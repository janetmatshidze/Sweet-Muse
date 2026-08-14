namespace Tuckshop.Core.App.Services
{
  using Neo.Identity;
  using System;
  using System.Threading.Tasks;
  using Tuckshop.Core.Models;
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
  }
}
