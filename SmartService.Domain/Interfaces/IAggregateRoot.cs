using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartService.Domain.Interfaces
{
    /// <summary>
    /// Marker interface to indicate an Aggregate Root
    /// in the Domain Driven Design context.
    /// 
    /// Used to enforce architectural boundaries
    /// between aggregates and other entities.
    /// </summary>

    public interface IAggregateRoot { }
}