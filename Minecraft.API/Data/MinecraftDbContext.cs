using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Minecraft.API.Data;

public class MinecraftDbContext : IdentityDbContext<IdentityUser>
{
    public MinecraftDbContext(DbContextOptions<MinecraftDbContext> options): base(options)
    {

    }

}