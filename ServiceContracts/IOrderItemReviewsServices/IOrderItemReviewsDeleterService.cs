using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.IOrderItemReviewsServices
{
    public interface IOrderItemReviewsDeleterService
    {
        Task<bool> DeleteOrderItemReview(Guid OrderId, Guid orderItemReviewId);
    }
}
