using IngSystem.AuthCore.Infrastructure.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IngSystem.AuthCore.Infrastructure
{
    /// <summary>
    /// 用于删除 Refresh Token,
    /// 因为当用户登录不同的设备就会生成不同的 Refresh Token
    /// 而这些 Refresh Token 都是被存在数据库里或者 Static store 里
    /// 不同的登录设备就 变成many to one 关系, 
    /// 如果想保证这些 refresh token 还能在不同的设备使用, 所以就要保存他们
    /// 而这个class 就是让那些refresh token 已经expire 的 删除数据库
    /// </summary>
    public class JwtRefreshTokenCache : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IJwtAuthManager _jwtAuthManager;

        public JwtRefreshTokenCache(IJwtAuthManager jwtAuthManager)
        {
            _jwtAuthManager = jwtAuthManager;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            // remove expired refresh tokens from cache every minute
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _jwtAuthManager.RemoveExpiredRefreshTokens(DateTime.Now);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
