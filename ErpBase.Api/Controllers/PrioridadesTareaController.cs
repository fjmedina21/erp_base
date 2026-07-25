using ErpBase.Api.Authorization;
using ErpBase.Api.Data;
using ErpBase.Api.DTOs.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBase.Api.Controllers;

[ApiController]
[Authorize]
[Tags("Catalog")]
[Route("api/prioridades-tarea")]
public sealed class PrioridadesTareaController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [HasPermission("prioridades_tarea.ver")]
    public async Task<ActionResult<IReadOnlyCollection<CatalogoBasicoResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await dbContext.PrioridadesTarea.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Nombre)
            .Select(x => new CatalogoBasicoResponse(x.Id, x.Nombre))
            .ToArrayAsync(cancellationToken));
}
