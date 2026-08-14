namespace Tuckshop.Core.Models.Wallets.Enums
{
  /// <summary>
  /// Represents the type of transaction that occurred in a customer's wallet.
  /// </summary>
  public enum WalletTransactionType
  {
    /// <summary>
    /// A deposit or top-up of funds into the wallet.
    /// </summary>
    Deposit,

    /// <summary>
    /// A withdrawal of funds from the wallet.
    /// </summary>  
    Withdrawal,

    /// <summary>
    /// A payment made for an order using the wallet balance.
    /// </summary>
    OrderPayment,
  }
}
