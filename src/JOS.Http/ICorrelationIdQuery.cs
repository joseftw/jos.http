using System;
using System.Threading.Tasks;

namespace JOS.Http;

public interface ICorrelationIdQuery
{
    Task<string> Execute();
}
