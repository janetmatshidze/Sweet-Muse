namespace Tuckshop.Core.App.Services
{
  using Microsoft.EntityFrameworkCore;
  using Neo.Extensions;
  using Neo.Identity;
  using Neo.Model.Exceptions;
  using System;
  using System.Threading.Tasks;
  using Tuckshop.Core.Models;
  using Tuckshop.Core.Models.Customers;
  using Tuckshop.Core.Models.Customers.Commands;
  using Tuckshop.Core.Models.Identity;
  using Tuckshop.Core.Models.Wallets.Commands;

  /// <summary>
  ///  Customers command Service that handles wallet deposits and withdrawals for customers. 
  /// </summary>
  public class CustomersCommandService
  {
    private readonly ModelDbContext dbContext;
    private readonly IUserResolver<User> userResolver;

    public CustomersCommandService(ModelDbContext dbContext, IUserResolver<User> userResolver)
    {
      this.dbContext = dbContext;
      this.userResolver = userResolver;
    }

    /// <summary>
    /// Deposits an amount into a customer's wallet.
    /// </summary>
    /// <param name="command">The deposit command.</param>
    /// <returns>The updated customer.</returns>
    public async Task<Customer> DepositAsync(DepositToWallet command)
    {
      return await this.ProcessWalletCommand(
        command.CustomerId,
        (customer, user) => customer.Deposit(command.Amount, user.UserId))
        .ConfigureAwait(false);

    }
    /// <summary>
    /// Withdraws an amount from a customer's wallet.
    /// </summary>
    /// <param name="command">The withdrawal command.</param>
    /// <returns>The updated customer.</returns>
    public async Task<Customer> WithdrawAsync(WithdrawFromWallet command)
    {
      return await this.ProcessWalletCommand(
        command.CustomerId,
        (customer, user) => customer.Withdraw(command.Amount, user.UserId))
        .ConfigureAwait(false);
    }

    private async Task<Customer> ProcessWalletCommand(int customerId, Action<Customer, User> handler)
    {
      var customer = await this.dbContext.Customers.FindAsync(customerId).ConfigureAwait(false);
      var user = await this.userResolver.GetUserAsync().ConfigureAwait(false);
      handler(customer, user);
      await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
      return customer;
    }

    public async Task<Customer> CreateCustomerAsync(CreateCustomer command)
    {

      var customer = new Customer();
      customer.UpdateDetails(command.FirstName, command.LastName, command.Email, command.PhoneNumber);

      this.dbContext.Customers.Add(customer);
      await this.dbContext.SaveChangesAsync();

      return customer;
    }

    public async Task<Customer> UpdateDetailsAsync(UpdateCustomerDetails command)
    {

      var customer = await this.dbContext.Customers.FindAsync(command.CustomerId).ConfigureAwait(false);

      customer.UpdateDetails(
        command.FirstName,
        command.LastName,
        command.Email,
        command.PhoneNumber

        );

      await this.dbContext.SaveChangesAsync().ConfigureAwait(false);

      return customer;
    }

    /// <summary>
    /// Deletes the customer identified by the provided command after verifying there are no associated orders.
    /// </summary>
    /// <remarks>Performs a database lookup and saves changes. An exception may be thrown if no customer with
    /// the specified identifier exists.</remarks>
    /// <param name="command">The command containing the identifier of the customer to delete.</param>
    /// <returns>A task that represents the asynchronous operation; completes when the customer has been removed and changes have
    /// been persisted.</returns>
    /// <exception cref="InvalidDomainOperationException">Thrown when the customer has existing orders and cannot be deleted.</exception>
    public async Task DeleteCustomerAsync(DeleteCustomer command)
    {
      var hasOrders = await this.dbContext.Orders.AnyAsync(o => o.CustomerId == command.CustomerId).ConfigureAwait(false);

      if (hasOrders)
      {
        throw new InvalidDomainOperationException(
          "This customer cannot be deleted because they have existing orders.");
      }

      var customer = await this.dbContext.Customers.FindAsync(command.CustomerId).ConfigureAwait(false);

      this.dbContext.Customers.Remove(customer);

      await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
  }
}

