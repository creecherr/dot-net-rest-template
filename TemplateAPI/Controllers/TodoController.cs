using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TemplateAPI.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using TemplateAPI.Services;
using TemplateAPI.Repositories;
using System;

namespace TemplateAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoRepository _todoRepository;
        private readonly ILogger<TodoItemsController> _logger;
        private IAuthService _authService;
        private readonly IMapper _mapper;

        public TodoItemsController(ITodoRepository todoRepository,
                                   ILogger<TodoItemsController> logger,
                                   IAuthService authService,
                                   IMapper mapper)
        {
            _todoRepository = todoRepository;
            _logger = logger;
            _authService = authService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public ActionResult Authenticate([FromBody]AuthUser model)
        {
            try
            {
                var user = _authService.Authenticate(model.Username, model.Password);

                if (user == null)
                    return BadRequest(new { message = "Username or password is incorrect" });

                return Ok(user);
            } catch (Exception ex) {
                _logger.LogError(String.Concat("An unhanded error occured when authenticating: ", ex.Message));
                return StatusCode(500);
            }

        }

        // GET: api/TodoItems
        [HttpGet]
        public ActionResult<IEnumerable<TodoItemResponseDTO>> GetTodoItems([FromQuery] QueryParameters queryParameters)
        {
            try
            {
                List<TodoEntity> items = _todoRepository.GetAll(queryParameters).ToList();

                var allItemCount = _todoRepository.Count();
                var totalPageCount = Math.Ceiling(allItemCount / (double)queryParameters.PageCount);
                var paginationMetadata = new
                {
                    totalCount = allItemCount,
                    pageSize = queryParameters.PageCount,
                    currentPage = queryParameters.Page,
                    totalPages = totalPageCount
                };

                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

                return Ok(new
                {
                    value = items
                });
            } catch (Exception ex) {
                _logger.LogError(String.Concat("An unhanded error occured when getting items: ", ex.Message));
                return StatusCode(500);
            }

        }

        // GET: api/TodoItems/5
        [HttpGet("{id}")]
        public ActionResult<TodoItemResponseDTO> GetTodoItem(int id)
        {
            try
            {
                TodoEntity todoItem = _todoRepository.GetSingle(id);

                if (todoItem == null)
                {
                    return NotFound();
                }

                return _mapper.Map<TodoItemResponseDTO>(todoItem);
            } catch (Exception ex) {
                _logger.LogError(String.Concat("An unhanded error occured when getting an item: ", ex.Message));
                return StatusCode(500);
            }
            
        }

        // POST: api/TodoItems
        [HttpPost]
        public ActionResult<TodoItemResponseDTO> PostTodoItem(TodoItemUpsertDTO todoItem)
        {
            try
            {
                TodoEntity item = _mapper.Map<TodoEntity>(todoItem);
                item.Id = _todoRepository.Count() + 1;
                _todoRepository.Add(item);
                _todoRepository.Save();

                TodoItemResponseDTO responseItem = _mapper.Map<TodoItemResponseDTO>(item);
                return CreatedAtAction(nameof(GetTodoItem), new { id = item.Id }, responseItem);
            } catch (Exception ex) {
                _logger.LogError(String.Concat("An unhandled error occured when adding items: ", ex.Message));
                return StatusCode(500);
            }

        }

        // PUT: api/TodoItems/5
        [HttpPut("{id}")]
        public ActionResult<TodoItemResponseDTO> PutTodoItem(int id, TodoItemUpsertDTO todoItem)
        {
            try
            {
                if (todoItem == null)
                {
                    return BadRequest();
                }

                var existingItem = _todoRepository.GetSingle(id);

                if (existingItem == null)
                {
                    return NotFound();
                }

                TodoEntity item = _mapper.Map<TodoEntity>(todoItem);
                //todo look at me 
                _todoRepository.Update(id, item);

                if (!_todoRepository.Save())
                {
                    throw new Exception("Updating a fooditem failed on save.");
                }

                TodoItemResponseDTO responseItem = _mapper.Map<TodoItemResponseDTO>(item);
                return responseItem;
            } catch (Exception ex) {

                _logger.LogError(String.Concat("An unhanded error occured when updating items: ", ex.Message));
                return StatusCode(500);
            }
        }

        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        public IActionResult DeleteTodoItem(int id)
        {
            try
            {
                TodoEntity todoItem = _todoRepository.GetSingle(id);

                if (todoItem == null)
                {
                    return NotFound();
                }

                _todoRepository.Delete(todoItem);

                if (!_todoRepository.Save())
                {
                    throw new Exception("Deleting a todo failed on save.");
                }
                return NoContent();
            } catch (Exception ex)
            {
                _logger.LogError(String.Concat("An unhanded error occured when deleteing items: ", ex.Message));
                return StatusCode(500);
            }
            
        }
    }
}
