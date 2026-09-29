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

    // Вызов нового эндпоинта

    public async Task<ExperimentDto?> FindByParametersAsync(
        string slug, int maxN, int step, int runs)
    {
        var response = await _http.GetAsync(
            $"api/experiments/search?slug={slug}&maxN={maxN}&step={step}&runs={runs}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null; // Кэш не найден
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ExperimentDto>();
    }
}