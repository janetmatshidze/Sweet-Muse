namespace Tuckshop.Core.Models.Wallets.Commands
{
  using Neo.Model;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  /// <summary>
  /// Class Representing a withdrawal from a customer's wallet.
  /// </summary>
  public class WithdrawFromWallet : ModelBase<WithdrawFromWallet>
  {
    [Required]
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets and sets the Amount to withdraw.
    /// </summary>
    [Column(TypeName = "money")]
    public decimal Amount { get; set; }

    ///// <inheritdoc/>
    //protected override void AddBusinessRules(ValidationRules<WithdrawFromWallet> rules)
    //{
    //  rules.FailWhen(c => c.Amount <= 0, "Withdrawal amount must be greater than zero.");
    //}
  }
}
