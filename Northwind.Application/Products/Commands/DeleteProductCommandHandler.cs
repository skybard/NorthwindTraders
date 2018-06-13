﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Northwind.Application.Exceptions;
using Northwind.Domain;
using Northwind.Persistence;

namespace Northwind.Application.Products.Commands
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
    {
        private readonly NorthwindDbContext _context;

        public DeleteProductCommandHandler(NorthwindDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Products.FindAsync(request.Id);

            if (entity == null)
            {
                throw new EntityNotFoundException(nameof(Product), request.Id);
            }

            var hasOrders = _context.OrderDetails.Any(od => od.ProductId == entity.ProductId);
            if (hasOrders)
            {
                throw new EntityDeleteFailureException(nameof(Product), request.Id, "There are existing orders associated with this product.");
            }

            _context.Products.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
