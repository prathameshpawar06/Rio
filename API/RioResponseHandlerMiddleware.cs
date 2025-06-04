namespace API
{
    public class RioResponseHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public RioResponseHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                var responseMessage = new { Error = 401, Message ="Please provide valid authentication credentials." };
                await context.Response.WriteAsJsonAsync(responseMessage);
            }
            else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                var responseMessage = new { Error = 403, Message ="You don't have the permissions to access this." };
                await context.Response.WriteAsJsonAsync(responseMessage);
            }
        }
    }
}
