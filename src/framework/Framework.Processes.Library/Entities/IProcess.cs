using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;

public interface IProcess<TProcessTypeId> : ILockableEntity
{
    Guid Id { get; }
    TProcessTypeId ProcessTypeId { get; set; }
}
