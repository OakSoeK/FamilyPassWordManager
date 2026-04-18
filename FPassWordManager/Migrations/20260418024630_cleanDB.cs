using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FPassWordManager.Migrations
{
    /// <inheritdoc />
    public partial class cleanDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PhNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PinHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActive = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
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
                name: "Credentials",
                columns: table => new
                {
                    CredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CredentialName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEditedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => x.CredentialId);
                    table.ForeignKey(
                        name: "FK_Credentials_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CredentialAccesses",
                columns: table => new
                {
                    CredentialAccessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SharedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SharedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredentialAccesses", x => x.CredentialAccessId);
                    table.ForeignKey(
                        name: "FK_CredentialAccesses_AspNetUsers_SharedByUserId",
                        column: x => x.SharedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CredentialAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CredentialAccesses_Credentials_CredentialId",
                        column: x => x.CredentialId,
                        principalTable: "Credentials",
                        principalColumn: "CredentialId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditCards",
                columns: table => new
                {
                    CreditDebitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardHolderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CardNumberHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ExpiryMonth = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ExpiryYear = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CvvHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    PinHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    BillingAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EditorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCards", x => x.CreditDebitId);
                    table.ForeignKey(
                        name: "FK_CreditCards_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditCards_AspNetUsers_EditorId",
                        column: x => x.EditorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditCards_Credentials_CredentialId",
                        column: x => x.CredentialId,
                        principalTable: "Credentials",
                        principalColumn: "CredentialId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityKeys",
                columns: table => new
                {
                    SecurityKeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PinHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EditorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityKeys", x => x.SecurityKeyId);
                    table.ForeignKey(
                        name: "FK_SecurityKeys_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SecurityKeys_AspNetUsers_EditorId",
                        column: x => x.EditorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SecurityKeys_Credentials_CredentialId",
                        column: x => x.CredentialId,
                        principalTable: "Credentials",
                        principalColumn: "CredentialId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebCredentials",
                columns: table => new
                {
                    WebCredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EditorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebCredentials", x => x.WebCredentialId);
                    table.ForeignKey(
                        name: "FK_WebCredentials_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebCredentials_AspNetUsers_EditorId",
                        column: x => x.EditorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebCredentials_Credentials_CredentialId",
                        column: x => x.CredentialId,
                        principalTable: "Credentials",
                        principalColumn: "CredentialId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditCardsHistorys",
                columns: table => new
                {
                    CreditDebitHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditDebitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardHolderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CardNumberHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ExpiryMonth = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ExpiryYear = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CvvHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    PinHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    BillingAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCardsHistorys", x => x.CreditDebitHistoryId);
                    table.ForeignKey(
                        name: "FK_CreditCardsHistorys_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditCardsHistorys_CreditCards_CreditDebitId",
                        column: x => x.CreditDebitId,
                        principalTable: "CreditCards",
                        principalColumn: "CreditDebitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditDebitCardAccesses",
                columns: table => new
                {
                    CreditDebitAccessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditDebitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SharedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SharedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditDebitCardAccesses", x => x.CreditDebitAccessId);
                    table.ForeignKey(
                        name: "FK_CreditDebitCardAccesses_AspNetUsers_SharedByUserId",
                        column: x => x.SharedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditDebitCardAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditDebitCardAccesses_CreditCards_CreditDebitId",
                        column: x => x.CreditDebitId,
                        principalTable: "CreditCards",
                        principalColumn: "CreditDebitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityKeyAccesses",
                columns: table => new
                {
                    SecurityKeyAccessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecurityKeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SharedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SharedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityKeyAccesses", x => x.SecurityKeyAccessId);
                    table.ForeignKey(
                        name: "FK_SecurityKeyAccesses_AspNetUsers_SharedByUserId",
                        column: x => x.SharedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SecurityKeyAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SecurityKeyAccesses_SecurityKeys_SecurityKeyId",
                        column: x => x.SecurityKeyId,
                        principalTable: "SecurityKeys",
                        principalColumn: "SecurityKeyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityKeysHistorys",
                columns: table => new
                {
                    SecurityKeyHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecurityKeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PinHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityKeysHistorys", x => x.SecurityKeyHistoryId);
                    table.ForeignKey(
                        name: "FK_SecurityKeysHistorys_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SecurityKeysHistorys_SecurityKeys_SecurityKeyId",
                        column: x => x.SecurityKeyId,
                        principalTable: "SecurityKeys",
                        principalColumn: "SecurityKeyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebCredentialAccesses",
                columns: table => new
                {
                    WebCredentialAccessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebCredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SharedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SharedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebCredentialAccesses", x => x.WebCredentialAccessId);
                    table.ForeignKey(
                        name: "FK_WebCredentialAccesses_AspNetUsers_SharedByUserId",
                        column: x => x.SharedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebCredentialAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebCredentialAccesses_WebCredentials_WebCredentialId",
                        column: x => x.WebCredentialId,
                        principalTable: "WebCredentials",
                        principalColumn: "WebCredentialId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebCredentialsHistorys",
                columns: table => new
                {
                    WebCredentialHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebCredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebCredentialsHistorys", x => x.WebCredentialHistoryId);
                    table.ForeignKey(
                        name: "FK_WebCredentialsHistorys_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebCredentialsHistorys_WebCredentials_WebCredentialId",
                        column: x => x.WebCredentialId,
                        principalTable: "WebCredentials",
                        principalColumn: "WebCredentialId",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_CredentialAccesses_CredentialId",
                table: "CredentialAccesses",
                column: "CredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_CredentialAccesses_SharedByUserId",
                table: "CredentialAccesses",
                column: "SharedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CredentialAccesses_UserId",
                table: "CredentialAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_UserId",
                table: "Credentials",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCards_CreatorId",
                table: "CreditCards",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCards_CredentialId",
                table: "CreditCards",
                column: "CredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCards_EditorId",
                table: "CreditCards",
                column: "EditorId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardsHistorys_ChangedByUserId",
                table: "CreditCardsHistorys",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardsHistorys_CreditDebitId",
                table: "CreditCardsHistorys",
                column: "CreditDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditDebitCardAccesses_CreditDebitId",
                table: "CreditDebitCardAccesses",
                column: "CreditDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditDebitCardAccesses_SharedByUserId",
                table: "CreditDebitCardAccesses",
                column: "SharedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditDebitCardAccesses_UserId",
                table: "CreditDebitCardAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityKeyAccesses_SecurityKeyId",
                table: "SecurityKeyAccesses",
                column: "SecurityKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityKeyAccesses_SharedByUserId",
                table: "SecurityKeyAccesses",
                column: "SharedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityKeyAccesses_UserId",
                table: "SecurityKeyAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityKeys_CreatorId",
                table: "SecurityKeys",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityKeys_CredentialId",
                table: "SecurityKeys",
                column: "CredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityKeys_EditorId",
                table: "SecurityKeys",
                column: "EditorId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityKeysHistorys_ChangedByUserId",
                table: "SecurityKeysHistorys",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityKeysHistorys_SecurityKeyId",
                table: "SecurityKeysHistorys",
                column: "SecurityKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_WebCredentialAccesses_SharedByUserId",
                table: "WebCredentialAccesses",
                column: "SharedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WebCredentialAccesses_UserId",
                table: "WebCredentialAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WebCredentialAccesses_WebCredentialId",
                table: "WebCredentialAccesses",
                column: "WebCredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_WebCredentials_CreatorId",
                table: "WebCredentials",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_WebCredentials_CredentialId",
                table: "WebCredentials",
                column: "CredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_WebCredentials_EditorId",
                table: "WebCredentials",
                column: "EditorId");

            migrationBuilder.CreateIndex(
                name: "IX_WebCredentialsHistorys_ChangedByUserId",
                table: "WebCredentialsHistorys",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WebCredentialsHistorys_WebCredentialId",
                table: "WebCredentialsHistorys",
                column: "WebCredentialId");
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
                name: "CredentialAccesses");

            migrationBuilder.DropTable(
                name: "CreditCardsHistorys");

            migrationBuilder.DropTable(
                name: "CreditDebitCardAccesses");

            migrationBuilder.DropTable(
                name: "SecurityKeyAccesses");

            migrationBuilder.DropTable(
                name: "SecurityKeysHistorys");

            migrationBuilder.DropTable(
                name: "WebCredentialAccesses");

            migrationBuilder.DropTable(
                name: "WebCredentialsHistorys");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "CreditCards");

            migrationBuilder.DropTable(
                name: "SecurityKeys");

            migrationBuilder.DropTable(
                name: "WebCredentials");

            migrationBuilder.DropTable(
                name: "Credentials");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
