using ForgeScheduler.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeScheduler.Application.Abstractions
{
    public interface IJobHandler
    {
        string JobType { get; }

        Task HandleAsync(Job job, CancellationToken cancellationToken);
    }
}
