using Microsoft.AspNetCore.Mvc;
using MovieApi.DTOs;
using MovieApi.Repositories;
using MovieApi.SqlQueries;

namespace MovieApi.Controllers;

[ApiController]
[Route("[controller]")]
public class GenreController : ControllerBase
{
	private readonly IMemberRepository _mRepo;
	public GenreController(IMemberRepository mRepo) =>
		_mRepo = mRepo;
	
	
}