namespace Tuckshop.Core.Api.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using System.Threading.Tasks;
  using Tuckshop.Core.App.Services;
  using Tuckshop.Core.Models;
  using Tuckshop.Core.Models.Customers.Commands;
  using Tuckshop.Core.Models.Wallets.Commands;

  /// <summary>
  /// Represets a controller for handling customer-related commands.
  /// </summary>
  [ApiController]
  [Route("api/customers/commands")]
  public class CustomersCommandController : ControllerBase
  {
    private readonly CustomersCommandService customersCommandService;

    /// <summary>
    /// Creates a new instance of the <see cref="CustomersCommandController"/> class.
    /// </summary>
    /// <param name="customersCommandService">Customers command service.</param>
    public CustomersCommandController(CustomersCommandService customersCommandService)
    {
      this.customersCommandService = customersCommandService;
    }

    /// <summary>
    /// Deposits an amount into a customer's wallet.
    /// </summary>
    /// <param name="command">The deposit command.</param>
    /// <returns>The updated customer.</returns>
    [HttpPost("deposit")]
    public async Task<Customer> Deposit([FromBody] DepositToWallet command)
    {
      return await this.customersCommandService.DepositAsync(command);
    }

    /// <summary>
    /// Withdraws an amount from a customer's wallet.
    /// </summary>
    /// <param name="command">The withdraw command.</param>
    /// <returns>The updated customer.</returns>
    [HttpPost("withdraw")]
    public async Task<Customer> Withdraw([FromBody] WithdrawFromWallet command)
    {
      return await this.customersCommandService.WithdrawAsync(command);
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="command">The create customer command.</param>
    /// <returns>The created customer.</returns>
    [HttpPost("create")]
    public async Task<Customer> Create([FromBody] CreateCustomer command)
    {
      return await this.customersCommandService.CreateCustomerAsync(command);
    }

    /// <summary>
    /// Updates a customer's profile details (name, email, phone number).
    /// Does not affect wallet balance.
    /// </summary>
    /// <param name="command">The update details command.</param>
    /// <returns>The updated customer.</returns>
    [HttpPut("update-details")]
    public async Task<Customer> UpdateDetails([FromBody] UpdateCustomerDetails command)
    {
      return await this.customersCommandService.UpdateDetailsAsync(command);
    }

    /// <summary>
    /// Deletes a customer however if customers has existing orders it refuses.
    /// </summary>
    /// <param name="command">Delete command</param>
    /// <returns>The deleted customer.</returns>
    [HttpPost("delete")]
    public async Task Delete([FromBody] DeleteCustomer command)
    {
      await this.customersCommandService.DeleteCustomerAsync(command);
    }
  }
}
