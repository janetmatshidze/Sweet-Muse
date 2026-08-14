namespace Tuckshop.Core.Models.Wallets
{
  using Neo.Model;
  using Neo.Model.Exceptions;
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Text;
  using Tuckshop.Core.Models.Wallets.Enums;

  /// <summary>
  /// Represents a single movement of funds in or out of a customer's wallet.
  /// </summary>
  public class WalletTransaction : ModelBase<WalletTransaction>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="WalletTransaction"/> class.
    /// </summary>
    private WalletTransaction()
    {
    }

    /// <summary>
    /// Gets the Wallet Transaction Id.
    /// </summary>
    public int WalletTransactionId { get; private set; }

    /// <summary>
    /// Gets the Customer Id.
    /// </summary>
    public int CustomerId { get; private set; }

    /// <summary>
    /// Gets the signed amount of the transaction. Positive for a deposit,
    /// negative for a withdrawal or an order payment.
    /// </summary>
    [Column(TypeName = "money")]
    public decimal Amount { get; private set; }

    /// <summary>
    /// Gets the type of transaction.
    /// </summary>
    public WalletTransactionType Type { get; private set; }

    /// <summary>
    /// Gets when the transaction occured.
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime OccurredOn { get; private set; }

    /// <summary>
    /// Gets the Id of the user who processed the transaction.
    /// </summary>
    public int ProcessedByUserId { get; private set; }

    /// <summary>
    /// Gets the Id of the order associated with the transaction, if applicable.
    /// </summary>
    public int? OrderId { get; private set; }

    internal WalletTransaction(int customerId, decimal amount, WalletTransactionType type, int processedByUserId, int? orderId = null)
    {
      if (amount == 0)
      {
        throw new InvalidDomainOperationException("Wallet transaction amount cannot be zero.");
      }

      this.CustomerId = customerId;
      this.Amount = amount;
      this.Type = type;
      this.ProcessedByUserId = processedByUserId;
      this.OrderId = orderId;
      this.OccurredOn = DateTime.UtcNow;
      this.TrackingState = TrackableEntities.Common.Core.TrackingState.Added;
    }
    /// <inheritdoc/>
    public override string ToString()
    {
      return $"{this.Type}: {this.Amount:C} for customer {this.CustomerId} on {this.OccurredOn:dd-MMM-yy HH:mm}";
    }
  }
}
