namespace Tuckshop.Core.Api.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using System.Threading.Tasks;
  using Tuckshop.Core.App.Services;
  using Tuckshop.Core.Models;
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
  }
}
