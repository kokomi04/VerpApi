using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VErp.Infrastructure.EF.EFExtensions
{
    public class MultipleDbTransaction : IDisposable, IAsyncDisposable, IDbContextTransaction
    {
        private readonly IList<IDbContextTransaction> transactions;

        public Guid TransactionId => throw new NotImplementedException();

        public MultipleDbTransaction(params DbContext[] contexts)
        {
            transactions = new List<IDbContextTransaction>();
            foreach (var ctx in contexts)
            {
                transactions.Add(ctx.Database.BeginTransaction());
            }
        }

        public void Commit()
        {
            foreach (var trans in transactions)
            {
                trans.Commit();
            }
        }


        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var trans in transactions)
            {
                await trans.CommitAsync();
            }
        }

        public void Rollback()
        {
            foreach (var trans in transactions)
            {
                trans.Rollback();
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            foreach (var trans in transactions)
            {
                await trans.RollbackAsync();
            }
        }

        public void Dispose()
        {
            foreach (var trans in transactions)
            {
                trans.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var trans in transactions)
            {
                await trans.DisposeAsync();
            }
        }


    }
}
