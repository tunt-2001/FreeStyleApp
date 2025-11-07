using FreeStyleApp.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace FreeStyleApp.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode = (int)HttpStatusCode.InternalServerError; // Mã lỗi 500
            string errorMessage = "Đã xảy ra lỗi không mong muốn từ máy chủ. Vui lòng thử lại sau.";

            switch (exception)
            {
                case AppException appEx:
                    statusCode = (int)HttpStatusCode.BadRequest; // Mã lỗi 400
                    errorMessage = appEx.Message;
                    _logger.LogWarning("Lỗi nghiệp vụ (Bad Request): {Message}", appEx.Message);
                    break;

                case DbUpdateException dbUpdateEx:
                    var innerException = dbUpdateEx.InnerException;

                    if (innerException is SqlException sqlEx && sqlEx.Number == 2627)
                    {
                        statusCode = (int)HttpStatusCode.BadRequest; // Lỗi 400
                        errorMessage = "Dữ liệu bạn nhập bị trùng lặp. Vui lòng kiểm tra lại.";
                        _logger.LogWarning("Vi phạm ràng buộc khóa duy nhất (UNIQUE KEY) trong CSDL. Chi tiết: {InnerMessage}", innerException.Message);
                    }
                    else
                    {
                        errorMessage = "Lỗi khi cập nhật cơ sở dữ liệu. Vui lòng thử lại.";
                        _logger.LogError(dbUpdateEx, "Lỗi DbUpdateException. Inner Exception: {InnerMessage}", innerException?.Message);
                    }
                    break;

                default:
                    _logger.LogError(exception, "Lỗi không xác định đã xảy ra: {Message}", exception.Message);
                    break;
            }

            context.Response.StatusCode = statusCode;

            var result = JsonSerializer.Serialize(new { message = errorMessage });
            await context.Response.WriteAsync(result);
        }
    }
}