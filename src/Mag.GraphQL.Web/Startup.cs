using Mag.Data;
using Mag.GraphQL.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NGraphQL.Server;
using NGraphQL.Server.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vita.Data;

namespace Mag.GraphQL.Web {
  public class Startup {
    public IConfiguration Configuration { get; }
    public static MitreGraphQLServer GraphQLServerInstance;
    public static GraphQLHttpServer GraphQLHttpServerInstance;

    public Startup(IConfiguration configuration) {
      this.Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services) {
      InitServer();

      services.AddControllers()
      // Note: by default System.Text.Json ns/serializer is used by AspNetCore 3.1; this serializer is no good -
      // - does not serialize fields, does not handle dictionaries, etc. So we put back Newtonsoft serializer.
      .AddNewtonsoftJson()
    // If your REST controllers reside in separate assembly, specify the assembly explicitly like that to make sure
    //  ASP.NET router finds these controllers
    //.PartManager.ApplicationParts.Add(new AssemblyPart(typeof(MyRestController).Assembly));
    ;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }
      app.UseCors(builder => builder
           .AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());
      app.UseHttpsRedirection();
      app.UseRouting();

      GraphQLHttpServerInstance = new GraphQLHttpServer(GraphQLServerInstance);
      app.UseEndpoints(endpoints => {
        endpoints.MapPost("graphql", HandleRequest);
        endpoints.MapGet("graphql", HandleRequest);
        endpoints.MapGet("graphql/schema", HandleRequest);
        endpoints.MapControllers(); //for RESTful endpoints
      });

      // Use GraphiQL UI
      app.UseGraphQLGraphiQL(path: "/");
    }

    private Task HandleRequest(HttpContext context) {
      return GraphQLHttpServerInstance.HandleGraphQLHttpRequestAsync(context);
    }


    // Initialization
    private void InitServer() {
      // init data app, then graphQL server on top of it
      MitreDataAppInit.InitConnectDataApp();
      // Note: in options, we do NOT enable parallel queries; everything is serial 
      var serverStt = new GraphQLServerSettings() { Options = GraphQLServerOptions.EnableRequestCache | GraphQLServerOptions.ReturnExceptionDetails };
      GraphQLServerInstance = new MitreGraphQLServer(MitreDataAppInit.MitreDataApp, serverStt);
    }




  }
}
