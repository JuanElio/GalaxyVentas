namespace Ventas.Application.Comun;

public interface IUnitOfWork
{
    Task GuardarCambiosAsync(CancellationToken ct);
}
