using _03.Contexts;
using _03.Entities;
using _03.Helpers;
using _03.Models;
using _03.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace _03.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    //Validacion del Bearer
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    //[EnableCors("PermitirApiRequest")]
    public class AutoresController: ControllerBase
    {
        private readonly IDataProtector _protector;
        private readonly ApplicationDbContext context;
        private readonly ILogger<AutoresController> logger;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly HashService _hashService;
        public AutoresController(IDataProtectionProvider protectionProvider, 
            HashService hashService, 
            ApplicationDbContext context, 
            ILogger<AutoresController> logger, 
            IMapper mapper, 
            IConfiguration configuration)
        {
           

            this.context = context;
            this.logger = logger;
            this.mapper = mapper;
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _protector = protectionProvider.CreateProtector("valor_unico_y_quizas_secreto");
            _hashService = hashService;
        }

        [HttpGet("Primer")]
        public ActionResult<Autor> GetPrimerAutor()
        {
            return context.Autores.FirstOrDefault();
        }

        [HttpGet]
        [HttpGet("/listado")]
        [HttpGet("listado")]
        [ServiceFilter(typeof(MiFiltroDeAccion))]
        public async Task<ActionResult<IEnumerable<AutorDTO>>> Get()    
        {
            //throw new NotImplementedException();
            logger.LogInformation("autores get");
            var autores = await context.Autores.ToListAsync();
            var autoresDTO = mapper.Map<List<AutorDTO>>(autores);
            return autoresDTO;
        }

        [HttpGet("ValueConfigure")]
        public ActionResult GetValueConfigure()
        {
            //Obtener valores desde el configure
            var apellido = configuration["apellido"];
            var connection = configuration["connectionStrings:DefaultConnection"];

            return Ok(new { apellido, connection });
        }

        [HttpGet("hash")]
        public ActionResult GetHash()
        {
            string textoPlano = "name";
            var hashResult1 = _hashService.Hash(textoPlano).Hash;
            var hashResult2 = _hashService.Hash(textoPlano).Hash;
            return Ok(new { textoPlano, hashResult1, hashResult2 });
        }
        [HttpGet("Encriptacion")]
        public ActionResult<string> GetEncriptacion()    
        {
            string textoPlano = "name";
          
            string textoCifrado = _protector.Protect(textoPlano);
            string textoDesencriptado = _protector.Unprotect(textoCifrado);
            return Ok(new { textoPlano, textoCifrado, textoDesencriptado });
        }

        [HttpGet("GetEncriptacionTime")]
        public ActionResult<string> GetEncriptacionTime()
        {
            string textoPlano = "name";

            //delimitado por 5 segundos
            var protectorLimitadoPorTiempo = _protector.ToTimeLimitedDataProtector();
            string textoCifrado = protectorLimitadoPorTiempo.Protect(textoPlano, TimeSpan.FromSeconds(5));
            //espera 6s
            Thread.Sleep(6000);
            string textoDesencriptado = protectorLimitadoPorTiempo.Unprotect(textoCifrado);
            return Ok(new { textoPlano, textoCifrado, textoDesencriptado });
        }


        //[HttpPut("{id}/{param?}")]
        //[HttpPut("{id}/{param=nameParam}")]
        //los parametros viene de un valor de ruta
        //los querys string se mapean directo con los parametros
        [HttpGet("{id}", Name = "ObtenerAutor")]
        public async  Task<ActionResult<AutorDTO>> Get(int id)
        {
            var autor = await  context.Autores.FirstOrDefaultAsync(x => x.Id == id);
            if (autor == null)
            {
                logger.LogWarning("autores no encontrado");
                return NotFound();
            }
            var autorDTO = mapper.Map<AutorDTO>(autor);

            return autorDTO;
        }

        [HttpPost]

        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorCreacion)
        {
            // Esto no es necesario en asp.net core 2.1 en adelante
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            var autor = mapper.Map<Autor>(autorCreacion);
            context.Autores.Add(autor);
            await context.SaveChangesAsync();
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return new CreatedAtRouteResult("ObtenerAutor", new { id = autor.Id }, autorDTO);
        }

        
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] AutorCreacionDTO autorActualizacion)
        {
            var autor = mapper.Map<Autor>(autorActualizacion);
            autor.Id = id;
            context.Entry(autor).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<AutorPathDTO> patchDocument)
        {
            if (patchDocument == null) return BadRequest();
            
            var autorDeLaDB = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);

            if (autorDeLaDB == null) return NotFound();
            
            var autorDTO = mapper.Map<AutorPathDTO>(autorDeLaDB);

            patchDocument.ApplyTo(autorDTO, ModelState);

            mapper.Map(autorDTO, autorDeLaDB);
            var isValid = TryValidateModel(autorDeLaDB);

            if (!isValid) return BadRequest(ModelState);
            

            await context.SaveChangesAsync();

            return NoContent();

        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<Autor>> Delete(int id)
        {
            var autorId = await context.Autores.Select(x => x.Id).FirstOrDefaultAsync(x => x == id);
            if (autorId == default(int)) return NotFound();
            
            context.Remove(new Autor { Id = autorId });
            await context.SaveChangesAsync();
            return NoContent();
        }

    }
}
