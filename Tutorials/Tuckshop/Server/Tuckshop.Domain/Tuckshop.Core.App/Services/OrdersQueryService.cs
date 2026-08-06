namespace Tuckshop.Core.App.Services
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Tuckshop.Core.Models;
  using Tuckshop.Core.Models.Orders.Enums;
  using Tuckshop.Core.Models.Orders.Queries;

  /// <summary>
  /// Orders Query Service.
  /// </summary>
  public class OrdersQueryService
  {
    private readonly ModelDbContext dbContext;
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQueryService"/> class.
    /// </summary>
    /// <remarks>The caller is responsible for managing the lifetime of the provided DbContext.</remarks>
    /// <param name="dbContext">ModelDbContext used to access order data.</param>
    public OrdersQueryService(ModelDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    /// <summary>
    /// Gets the orders for the given criteria.
    /// </summary>
    /// <param name="criteria">The order lookup citeria.</param>
    /// <returns>A query to retieve the orders from the database.</returns>
    public async Task<List<OrderLookup>> GetOrderLookupAsync(OrderLookupCriteria criteria)
    {
      var flatOrderList =
          await (from o in this.dbContext.Orders
                 from od in o.OrderDetails
                 join p in this.dbContext.Products on od.ProductId equals p.ProductId
                 join completedBy in this.dbContext.Users on o.Completed.By equals completedBy.UserId into completedByGroup
                 from completedBy in completedByGroup.DefaultIfEmpty()
                 join cancelledBy in this.dbContext.Users on o.Cancelled.By equals cancelledBy.UserId into cancelledByGroup
                 from cancelledBy in cancelledByGroup.DefaultIfEmpty()
                 where (criteria.OrderStatus == null
                     || (criteria.OrderStatus == OrderStatus.Pending && o.Completed.On == null && o.Cancelled.On == null)
                     || (criteria.OrderStatus == OrderStatus.Completed && o.Completed.On != null)
                     || (criteria.OrderStatus == OrderStatus.Cancelled && o.Cancelled.On != null))
                && (criteria.StartDate == null || o.OrderedOn >= criteria.StartDate)
                && (criteria.EndDate == null || o.OrderedOn < criteria.EndDate.Value.AddDays(1))
                 select new
                 {
                   Order = new OrderLookup()
                   {
                     OrderId = o.OrderId,
                     CustomerName = o.CustomerName,
                     OrderedOn = o.OrderedOn,
                     CompletedOn = o.Completed.On,
                     CancelledOn = o.Cancelled.On,
                     CompletedByFirstName = completedBy.FirstName,
                     CompletedByLastName = completedBy.LastName,
                     CancelledByFirstName = cancelledBy.FirstName,
                     CancelledByLastName = cancelledBy.LastName,
                     CancelledReason = o.Cancelled.Reason,
                     CompletedBy = completedBy.FirstName == null ? string.Empty : $"{completedBy.FirstName} {completedBy.LastName}",
                     CancelledBy = cancelledBy.FirstName == null ? string.Empty : $"{cancelledBy.FirstName} {cancelledBy.LastName}",
                   },
                   OrderDetail = new OrderDetailLookup()
                   {
                     Product = p.ProductName,
                     Price = od.Value / od.Quantity,
                     Value = od.Value,
                     VAT = od.VAT,
                   },
                 }).ToListAsync().ConfigureAwait(false);

      return flatOrderList
        .GroupBy(c => c.Order.OrderId)
        .Select(g => g.First().Order.WithDetails(g.Select(c => c.OrderDetail)))
        .ToList();
    }
  }
}

