namespace Tuckshop.Core.Models
{
  using Neo.Model;
  using Neo.Model.Exceptions;
  using Neo.Model.Validation;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Tuckshop.Core.Models.Wallets;
  using Tuckshop.Core.Models.Wallets.Enums;

  /// <summary>
  /// Customer class to represent a customer in the system.
  /// </summary>
  public class Customer : ModelBase<Customer>
  {
    /// <summary>
    /// Gets or sets the Customer Id.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the Customer Name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Customer Last Name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Email.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Phone Number.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(10)]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Wallet Balance, which represents the amount of money available in the customer's wallet.
    /// </summary>
    [Column(TypeName = "money")]
    public decimal WalletBalance { get; private set; }

    /// <summary>
    /// Gets the wallet transaction history for this customer.
    /// </summary>
    public List<WalletTransaction> WalletTransactions { get; private set; } = new List<WalletTransaction>();

    /// <summary>
    /// Deposits an amount into the customer's wallet.
    /// </summary>
    /// <param name="amount">The amount to deposit. Must be greater than zero.</param>
    /// <param name="processedByUserId">The ID of the user who processed the transaction.</param>
    /// <returns>The created wallet transaction.</returns>
    public WalletTransaction Deposit(decimal amount, int processedByUserId)
    {
      if (amount <= 0)
      {
        throw new InvalidDomainOperationException("Deposit amount must be greater than zero.");
      }
      var transaction = new WalletTransaction(this.CustomerId, amount, WalletTransactionType.Deposit, processedByUserId);
      this.WalletTransactions.Add(transaction);
      this.WalletBalance += amount;
      return transaction;
    }

    /// <summary>
    /// Withdraws the specified amount from the wallet balance.
    /// </summary>
    /// <param name="amount">Amount to withdraw. Must be greater than zero and not exceed the current wallet balance.</param>
    /// <exception cref="InvalidDomainOperationException">Thrown when amount is less than or equal to zero, or when the wallet balance is insufficient to cover the
    /// withdrawal.</exception>
    public WalletTransaction Withdraw(decimal amount, int processedByUserId)
    {
      if (amount <= 0)
      {
        throw new InvalidDomainOperationException("Withdrawal amount must be greater than zero.");
      }

      if (this.WalletBalance - amount < 0)
      {
        throw new InvalidDomainOperationException("Insufficient wallet balance for this withdrawal.");
      }

      var transaction = new WalletTransaction(this.CustomerId, -amount, WalletTransactionType.Withdrawal, processedByUserId);
      this.WalletTransactions.Add(transaction);
      this.WalletBalance -= amount;
      return transaction;
    }

    /// <summary>
    ///  Charges the customer's wallet for an order. Will not allow the balance to go negative.
    /// </summary>
    /// <param name="amount">The order total to charge</param>
    /// <param name="orderId">The order being paid for</param>
    /// <param name="processedByUserId">The user (or system) processing this charge.</param>
    /// <returns>The created wallet transaction.</returns>
    public WalletTransaction ChargeForOrder(decimal amount, int orderId, int processedByUserId)
    {
      if (amount <= 0)
      {
        throw new InvalidDomainOperationException("Order charge amount must be greater than zero.");
      }

      if (this.WalletBalance - amount < 0)
      {
        throw new InvalidDomainOperationException("Insufficient wallet balance for this order.");
      }

      var transaction = new WalletTransaction(this.CustomerId, -amount, WalletTransactionType.OrderPayment, processedByUserId, orderId);
      this.WalletTransactions.Add(transaction);
      this.WalletBalance -= amount;
      return transaction;
    }

    /// <summary>
    /// Updates the instance's contact details.
    /// </summary>
    /// <param name="firstName">New first name.</param>
    /// <param name="lastName">New last name.</param>
    /// <param name="email">New email address.</param>
    /// <param name="phoneNumber">New phone number.</param>
    public void UpdateDetails(string firstName, string lastName, string email, string phoneNumber)
    {
      this.FirstName = firstName;
      this.LastName = lastName;
      this.Email = email;
      this.PhoneNumber = phoneNumber;
    }

    /// <inheritdoc />
    protected override void AddBusinessRules(ValidationRules<Customer> rules)
    {
      rules.FailWhen(
        c => !System.Text.RegularExpressions.Regex.IsMatch(c.PhoneNumber ?? "", @"^\d{10}$"),
        "Phone number must be exactly 10 digits.");

      rules.FailWhen(
    c => System.Text.RegularExpressions.Regex.IsMatch(c.PhoneNumber ?? "", @"^\d{10}$")
         && System.Text.RegularExpressions.Regex.IsMatch(c.PhoneNumber ?? "", @"^(\d)\1{9}$"),
    "Please enter a valid phone number.");

      rules.FailWhen(
        c => !System.Text.RegularExpressions.Regex.IsMatch(c.Email ?? "", @"^[^\s@]+@[^\s@]+\.[^\s@]+$"),
        "Please enter a valid email address");
    }
  }
}

