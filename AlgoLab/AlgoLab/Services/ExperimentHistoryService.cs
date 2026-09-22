using System.Net.Http.Json;
using AlgoLab.Models;

namespace AlgoLab.Services;

public class ExperimentHistoryService
{
    private readonly HttpClient _http;


    public ExperimentHistoryService(
        HttpClient http)
    {
        _http = http;
    }


    public async Task<List<ExperimentDto>>
        GetAllAsync()
    {
        return await _http
            .GetFromJsonAsync<List<ExperimentDto>>(
                "api/experiments")
            ?? new List<ExperimentDto>();
    }


    public async Task<ExperimentDto?>
        GetByIdAsync(int id)
    {
        return await _http
            .GetFromJsonAsync<ExperimentDto>(
                $"api/experiments/{id}");
    }


    public async Task<ExperimentDto?>
        SaveAsync(
            ExperimentDto experiment)
    {
        var response =
            await _http.PostAsJsonAsync(
                "api/experiments",
                experiment);


        response.EnsureSuccessStatusCode();


        return await response.Content
            .ReadFromJsonAsync<ExperimentDto>();
    }


    public async Task DeleteAsync(
        int id)
    {
        var response =
            await _http.DeleteAsync(
                $"api/experiments/{id}");


        response.EnsureSuccessStatusCode();
    }
}