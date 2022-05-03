using seving.core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core
{
    public interface IApplyEvent<E> where E : StreamEvent 
    {
        public Task ApplyEvent(E @event, StreamRoot streamRoot);
    }
}
