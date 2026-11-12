using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FitManager.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ALUNO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cpf = table.Column<string>(type: "char(11)", nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "date", nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALUNO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EXERCICIO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    GrupoMuscular = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXERCICIO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "INSTRUTOR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cref = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INSTRUTOR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PLANO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DuracaoMeses = table.Column<int>(type: "int", nullable: false),
                    ValorMensal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PLANO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TREINO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    InstrutorId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Objetivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "date", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TREINO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TREINO_ALUNO_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "ALUNO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TREINO_INSTRUTOR_InstrutorId",
                        column: x => x.InstrutorId,
                        principalTable: "INSTRUTOR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MATRICULA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    PlanoId = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "date", nullable: false),
                    DataFim = table.Column<DateTime>(type: "date", nullable: false),
                    ValorMensal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataCancelamento = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MATRICULA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MATRICULA_ALUNO_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "ALUNO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MATRICULA_PLANO_PlanoId",
                        column: x => x.PlanoId,
                        principalTable: "PLANO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ITEM_TREINO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TreinoId = table.Column<int>(type: "int", nullable: false),
                    ExercicioId = table.Column<int>(type: "int", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Series = table.Column<int>(type: "int", nullable: false),
                    Repeticoes = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CargaKg = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    DescansoSegundos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ITEM_TREINO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ITEM_TREINO_EXERCICIO_ExercicioId",
                        column: x => x.ExercicioId,
                        principalTable: "EXERCICIO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ITEM_TREINO_TREINO_TreinoId",
                        column: x => x.TreinoId,
                        principalTable: "TREINO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAGAMENTO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatriculaId = table.Column<int>(type: "int", nullable: false),
                    MesReferencia = table.Column<DateTime>(type: "date", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "date", nullable: false),
                    FormaPagamento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAGAMENTO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PAGAMENTO_MATRICULA_MatriculaId",
                        column: x => x.MatriculaId,
                        principalTable: "MATRICULA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EXERCICIO",
                columns: new[] { "Id", "Descricao", "GrupoMuscular", "Nome" },
                values: new object[,]
                {
                    { 1, "Deitado no banco, descer a barra até o peito e empurrar.", "Peito", "Supino reto com barra" },
                    { 2, "Banco inclinado a 30-45 graus.", "Peito", "Supino inclinado com halteres" },
                    { 3, "Fechar os braços à frente do corpo.", "Peito", "Crucifixo na máquina (peck deck)" },
                    { 4, "Com o peso do corpo.", "Peito", "Flexão de braço" },
                    { 5, "Puxar a barra até a altura do peito.", "Costas", "Puxada frontal" },
                    { 6, "Puxar o triângulo em direção ao abdômen.", "Costas", "Remada baixa" },
                    { 7, "Tronco inclinado e coluna reta.", "Costas", "Remada curvada com barra" },
                    { 8, "Com o peso do corpo ou no gravitron.", "Costas", "Barra fixa" },
                    { 9, "Barra apoiada nos ombros.", "Pernas", "Agachamento livre" },
                    { 10, "Empurrar a plataforma sem travar os joelhos.", "Pernas", "Leg press 45°" },
                    { 11, "Isola o quadríceps.", "Pernas", "Cadeira extensora" },
                    { 12, "Isola os posteriores de coxa.", "Pernas", "Mesa flexora" },
                    { 13, "Elevação dos calcanhares.", "Pernas", "Panturrilha em pé" },
                    { 14, "Posteriores de coxa e glúteos.", "Pernas", "Stiff" },
                    { 15, "Empurrar os halteres acima da cabeça.", "Ombros", "Desenvolvimento com halteres" },
                    { 16, "Elevar os halteres até a linha dos ombros.", "Ombros", "Elevação lateral" },
                    { 17, "Elevar os halteres à frente do corpo.", "Ombros", "Elevação frontal" },
                    { 18, "Cotovelos fixos ao lado do corpo.", "Bíceps", "Rosca direta com barra" },
                    { 19, "Um braço de cada vez.", "Bíceps", "Rosca alternada com halteres" },
                    { 20, "Pegada neutra.", "Bíceps", "Rosca martelo" },
                    { 21, "Estender os cotovelos empurrando a barra para baixo.", "Tríceps", "Tríceps na polia (pulley)" },
                    { 22, "Deitado, descer a barra em direção à testa.", "Tríceps", "Tríceps testa" },
                    { 23, "Halter atrás da cabeça.", "Tríceps", "Tríceps francês" },
                    { 24, "Elevar o tronco contraindo o abdômen.", "Abdômen", "Abdominal supra" },
                    { 25, "Manter a posição pelo tempo indicado nas repetições.", "Abdômen", "Prancha isométrica" },
                    { 26, "Deitado ou na barra.", "Abdômen", "Elevação de pernas" },
                    { 27, "Caminhada ou corrida; informar o tempo nas repetições.", "Cardio", "Esteira" },
                    { 28, "Informar o tempo nas repetições.", "Cardio", "Bicicleta ergométrica" }
                });

            migrationBuilder.InsertData(
                table: "INSTRUTOR",
                columns: new[] { "Id", "Ativo", "Cref", "Email", "Nome", "Telefone" },
                values: new object[,]
                {
                    { 1, true, "012345-G/PR", "instrutor@fitmanager.com", "Carlos Henrique Souza", "(43) 99999-0001" },
                    { 2, true, "067890-G/PR", "mariana@fitmanager.com", "Mariana Alves Costa", "(43) 99999-0002" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ALUNO_Cpf",
                table: "ALUNO",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_INSTRUTOR_Cref",
                table: "INSTRUTOR",
                column: "Cref",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ITEM_TREINO_ExercicioId",
                table: "ITEM_TREINO",
                column: "ExercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ITEM_TREINO_TreinoId",
                table: "ITEM_TREINO",
                column: "TreinoId");

            migrationBuilder.CreateIndex(
                name: "IX_MATRICULA_AlunoId",
                table: "MATRICULA",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_MATRICULA_PlanoId",
                table: "MATRICULA",
                column: "PlanoId");

            migrationBuilder.CreateIndex(
                name: "IX_PAGAMENTO_MatriculaId_MesReferencia",
                table: "PAGAMENTO",
                columns: new[] { "MatriculaId", "MesReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TREINO_AlunoId",
                table: "TREINO",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_TREINO_InstrutorId",
                table: "TREINO",
                column: "InstrutorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ITEM_TREINO");

            migrationBuilder.DropTable(
                name: "PAGAMENTO");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "EXERCICIO");

            migrationBuilder.DropTable(
                name: "TREINO");

            migrationBuilder.DropTable(
                name: "MATRICULA");

            migrationBuilder.DropTable(
                name: "INSTRUTOR");

            migrationBuilder.DropTable(
                name: "ALUNO");

            migrationBuilder.DropTable(
                name: "PLANO");
        }
    }
}
