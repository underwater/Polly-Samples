using Polly;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPollyExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var waitSequence = new List<TimeSpan>(){
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5)
                    };

            var retryPolicy = Policy
                .Handle<NotImplementedException>()
                .WaitAndRetryAsync(waitSequence);

            var breakerPolicy = Policy
                      .Handle<NotImplementedException>()
                      .CircuitBreakerAsync(
                        exceptionsAllowedBeforeBreaking: 4,
                        durationOfBreak: TimeSpan.FromSeconds(5), 
                        onBreak: (err, ctx) =>  ColorConsole.WriteError($"Breaking !"), 
                        onReset: () =>  ColorConsole.WriteError($"Resetting !"));

            var wrapPolicy = Policy.WrapAsync(retryPolicy, breakerPolicy);

            try
            {
                //await wrapPolicy.ExecuteAsync(() => DoSomething("retry + circuit breaker policy"));

                //await retryPolicy.ExecuteAsync(() => DoSomething("retry policy"));

                // why passing context doesn't work ?
                //await breakerPolicy.ExecuteAsync(a => DoSomething(a.PolicyKey));

                // why circuit breaker doesn't work?
                await breakerPolicy.ExecuteAsync(() => DoSomething("circuit breaker policy"));

            }
            catch (Exception)
            {
                ColorConsole.WriteError($"Failed !");
                //throw;
            }



        }

      
        private async static Task DoSomething(string description)
        {   
        
            await Task.Delay(TimeSpan.FromSeconds(1));
            ColorConsole.WriteInfo($"work with {description}.... {DateTime.UtcNow}");
            throw new NotImplementedException();
        }
    }
}
