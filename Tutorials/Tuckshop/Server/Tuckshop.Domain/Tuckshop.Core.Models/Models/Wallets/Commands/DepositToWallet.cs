namespace Tuckshop.Core.Models.Wallets.Commands
{
  using Neo.Model;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  /// <summary>
  /// Class representing a deposit to a customer's wallet.
  /// </summary>
  public class DepositToWallet : ModelBase<DepositToWallet>
  {
    /// <summary>
    /// Gets and sets the Customer Id.
    /// </summary>
    [Required]
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets and sets the Amount to deposit.
    /// </summary>
    [Column(TypeName = "money")]
    public decimal Amount { get; set; }

    //// <inheritdoc/>
    //protected override void AddBusinessRules(ValidationRules<DepositToWallet> rules)
    //{
    //  rules.FailWhen(c => c.Amount <= 0, "Deposit amount must be greater than zero.");
    //}
  }
}