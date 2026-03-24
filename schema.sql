CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260111150951_InitialCreate', '8.0.4');

COMMIT;

START TRANSACTION;

CREATE TABLE "ActivityLogs" (
    "Id" uuid NOT NULL,
    "ServiceRequestId" uuid NOT NULL,
    "Action" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ActivityLogs" PRIMARY KEY ("Id")
);

CREATE TABLE "Assignments" (
    "Id" uuid NOT NULL,
    "ServiceRequestId" uuid NOT NULL,
    "AgentId" uuid NOT NULL,
    "EstimatedCost_Amount" numeric(18,2) NOT NULL,
    "EstimatedCost_Currency" character varying(10) NOT NULL,
    "AssignedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Assignments" PRIMARY KEY ("Id")
);

CREATE TABLE "ServiceAgents" (
    "Id" uuid NOT NULL,
    "FullName" text NOT NULL,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_ServiceAgents" PRIMARY KEY ("Id")
);

CREATE TABLE "ServiceCategories" (
    "Id" uuid NOT NULL,
    "Name" character varying(150) NOT NULL,
    "Description" text NOT NULL,
    CONSTRAINT "PK_ServiceCategories" PRIMARY KEY ("Id")
);

CREATE TABLE "ServiceFeedbacks" (
    "Id" uuid NOT NULL,
    "ServiceRequestId" uuid NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "Rating" integer NOT NULL,
    "Comment" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ServiceFeedbacks" PRIMARY KEY ("Id")
);

CREATE TABLE "ServiceRequests" (
    "Id" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "CategoryId" uuid NOT NULL,
    "Description" character varying(1000) NOT NULL,
    "ComplexityLevel" integer NOT NULL,
    "Status" integer NOT NULL,
    "AssignedProviderId" uuid,
    "EstimatedCost_Amount" numeric(18,2),
    "EstimatedCost_Currency" character varying(10),
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ServiceRequests" PRIMARY KEY ("Id")
);

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "FullName" character varying(200) NOT NULL,
    "Email" character varying(256) NOT NULL,
    "PhoneNumber" text NOT NULL,
    "Role" integer NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "AgentCapability" (
    "Id" uuid NOT NULL,
    "CategoryId" uuid NOT NULL,
    "MaxComplexityLevel" integer NOT NULL,
    "ServiceAgentId" uuid,
    CONSTRAINT "PK_AgentCapability" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AgentCapability_ServiceAgents_ServiceAgentId" FOREIGN KEY ("ServiceAgentId") REFERENCES "ServiceAgents" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MatchingResults" (
    "Id" uuid NOT NULL,
    "ServiceRequestId" uuid NOT NULL,
    "ServiceAgentId" uuid NOT NULL,
    "SupportedComplexityLevel" integer NOT NULL,
    "MatchingScore" numeric NOT NULL,
    "IsRecommended" boolean NOT NULL,
    "ServiceRequestId1" uuid,
    CONSTRAINT "PK_MatchingResults" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MatchingResults_ServiceRequests_ServiceRequestId" FOREIGN KEY ("ServiceRequestId") REFERENCES "ServiceRequests" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_MatchingResults_ServiceRequests_ServiceRequestId1" FOREIGN KEY ("ServiceRequestId1") REFERENCES "ServiceRequests" ("Id")
);

CREATE TABLE "ServiceAttachments" (
    "Id" uuid NOT NULL,
    "ServiceRequestId" uuid NOT NULL,
    "FileName" text NOT NULL,
    "FileUrl" text NOT NULL,
    "Type" integer NOT NULL,
    "UploadedAt" timestamp with time zone NOT NULL,
    "ServiceRequestId1" uuid,
    CONSTRAINT "PK_ServiceAttachments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ServiceAttachments_ServiceRequests_ServiceRequestId" FOREIGN KEY ("ServiceRequestId") REFERENCES "ServiceRequests" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ServiceAttachments_ServiceRequests_ServiceRequestId1" FOREIGN KEY ("ServiceRequestId1") REFERENCES "ServiceRequests" ("Id")
);

CREATE INDEX "IX_AgentCapability_ServiceAgentId" ON "AgentCapability" ("ServiceAgentId");

CREATE INDEX "IX_MatchingResults_ServiceRequestId" ON "MatchingResults" ("ServiceRequestId");

CREATE INDEX "IX_MatchingResults_ServiceRequestId1" ON "MatchingResults" ("ServiceRequestId1");

CREATE INDEX "IX_ServiceAttachments_ServiceRequestId" ON "ServiceAttachments" ("ServiceRequestId");

CREATE INDEX "IX_ServiceAttachments_ServiceRequestId1" ON "ServiceAttachments" ("ServiceRequestId1");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260111160242_Initial', '8.0.4');

COMMIT;

START TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260111160413_Initial1', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE "AgentCapability" DROP CONSTRAINT "FK_AgentCapability_ServiceAgents_ServiceAgentId";

ALTER TABLE "AgentCapability" DROP CONSTRAINT "PK_AgentCapability";

ALTER TABLE "AgentCapability" RENAME TO "AgentCapabilities";

ALTER INDEX "IX_AgentCapability_ServiceAgentId" RENAME TO "IX_AgentCapabilities_ServiceAgentId";

ALTER TABLE "ServiceRequests" ALTER COLUMN "Description" DROP NOT NULL;

ALTER TABLE "ServiceRequests" ALTER COLUMN "ComplexityLevel" DROP NOT NULL;

ALTER TABLE "AgentCapabilities" ADD CONSTRAINT "PK_AgentCapabilities" PRIMARY KEY ("Id");

ALTER TABLE "AgentCapabilities" ADD CONSTRAINT "FK_AgentCapabilities_ServiceAgents_ServiceAgentId" FOREIGN KEY ("ServiceAgentId") REFERENCES "ServiceAgents" ("Id") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260118192827_MakeComplexityNullable', '8.0.4');

COMMIT;

START TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260126084402_InitialCreate1', '8.0.4');

COMMIT;

START TRANSACTION;

CREATE TABLE "AuthData" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Email" character varying(256) NOT NULL,
    "PasswordHash" character varying(500) NOT NULL,
    "EncryptedRefreshToken" character varying(1000),
    "RefreshTokenExpiresAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_AuthData" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_AuthData_Email" ON "AuthData" ("Email");

CREATE UNIQUE INDEX "IX_AuthData_UserId" ON "AuthData" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260126200121_AddAuthData', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE "ServiceRequests" ADD "AddressText" text;

CREATE TABLE "ServiceAnalyses" (
    "Id" uuid NOT NULL,
    "ServiceRequestId" uuid NOT NULL,
    "ComplexityLevel" integer NOT NULL,
    "UrgencyLevel" integer NOT NULL,
    "SafetyAdvice" character varying(1000),
    "Summary" character varying(2000),
    "AnalyzedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ServiceAnalyses" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_ServiceAnalyses_ServiceRequestId" ON "ServiceAnalyses" ("ServiceRequestId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260127210253_AddServiceAnalysisAndAddressText', '8.0.4');

COMMIT;

START TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260223063939_I', '8.0.4');

COMMIT;

START TRANSACTION;

CREATE TABLE "ServiceDefinitions" (
    "Id" uuid NOT NULL,
    "CategoryId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Description" character varying(1000),
    "BasePrice" numeric(18,2) NOT NULL,
    "EstimatedDuration" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ServiceDefinitions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ServiceDefinitions_ServiceCategories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "ServiceCategories" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_ServiceDefinitions_CategoryId" ON "ServiceDefinitions" ("CategoryId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260225110511_AddServiceDefinition', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE "Users" ADD "IsLocked" boolean NOT NULL DEFAULT FALSE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260304164928_AddIsLockedToUser', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE "AuthData" ADD "PasswordResetToken" text;

ALTER TABLE "AuthData" ADD "PasswordResetTokenExpiresAt" timestamp with time zone;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260306150204_AddPasswordResetTokenToAuthData', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE "ServiceAgents" ADD "UserId" uuid;

ALTER TABLE "AgentCapabilities" ADD "ServiceDefinitionIds" jsonb NOT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260306163404_AddAgentCapabilityServiceDefsAndServiceAgentUserId', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE "AgentCapabilities" RENAME COLUMN "ServiceDefinitionIds" TO "ServiceIds";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260306170222_RenameServiceDefinitionIdsToServiceIds', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE "AgentCapabilities" DROP COLUMN "ServiceIds";

ALTER TABLE "AgentCapabilities" ADD "ServiceIds" uuid[] NOT NULL DEFAULT ARRAY[]::uuid[];

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260306171501_ChangeServiceIdsToUuidArray', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE "ServiceRequests" ADD "EstimatedDuration" character varying(255);

ALTER TABLE "ServiceRequests" ADD "EstimatedPrice" character varying(255);

ALTER TABLE "ServiceRequests" ADD "OcrExtractedText" text;

ALTER TABLE "ServiceRequests" ADD "WasAnalyzedByAI" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "ServiceDefinitions" ADD "ComplexityRange" integer[] NOT NULL DEFAULT (ARRAY[1,3]);

ALTER TABLE "ServiceDefinitions" ADD "IsDangerous" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "ServiceAnalyses" ADD "RiskExplanation" character varying(1000);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260307101624_AddAiAnalysisFields', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE "ServiceAnalyses" ADD "ProblemDiagnosis" character varying(2000);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260307112317_AddProblemDiagnosis', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE "ServiceRequests" ADD "ServiceDefinitionId" uuid;

CREATE INDEX "IX_ServiceRequests_ServiceDefinitionId" ON "ServiceRequests" ("ServiceDefinitionId");

ALTER TABLE "ServiceRequests" ADD CONSTRAINT "FK_ServiceRequests_ServiceDefinitions_ServiceDefinitionId" FOREIGN KEY ("ServiceDefinitionId") REFERENCES "ServiceDefinitions" ("Id") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260308125238_AddServiceDefinitionIdToServiceRequests', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE "ServiceRequests" ADD "CommissionAmount_Amount" numeric(18,2);

ALTER TABLE "ServiceRequests" ADD "CommissionAmount_Currency" character varying(10);

ALTER TABLE "ServiceRequests" ADD "CommissionRate" numeric(5,4) NOT NULL DEFAULT 0.0;

ALTER TABLE "ServiceRequests" ADD "DepositAmount_Amount" numeric(18,2);

ALTER TABLE "ServiceRequests" ADD "DepositAmount_Currency" character varying(10);

ALTER TABLE "ServiceRequests" ADD "FinalAmountPaid_Amount" numeric(18,2);

ALTER TABLE "ServiceRequests" ADD "FinalAmountPaid_Currency" character varying(10);

ALTER TABLE "ServiceRequests" ADD "FinalPrice_Amount" numeric(18,2);

ALTER TABLE "ServiceRequests" ADD "FinalPrice_Currency" character varying(10);

ALTER TABLE "ServiceRequests" ADD "IsDepositPaid" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "ServiceRequests" ADD "WorkerAmount_Amount" numeric(18,2);

ALTER TABLE "ServiceRequests" ADD "WorkerAmount_Currency" character varying(10);

CREATE TABLE "CommissionSettings" (
    "Id" uuid NOT NULL,
    "ServiceDefinitionId" uuid NOT NULL,
    "CommissionPercent" numeric(5,2) NOT NULL,
    "DepositPercent" numeric(5,2) NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CommissionSettings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CommissionSettings_ServiceDefinitions_ServiceDefinitionId" FOREIGN KEY ("ServiceDefinitionId") REFERENCES "ServiceDefinitions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "CompletionEvidences" (
    "Id" uuid NOT NULL,
    "ServiceRequestId" uuid NOT NULL,
    "WorkerId" uuid NOT NULL,
    "ImageUrl" text NOT NULL,
    "Notes" character varying(1000),
    "Type" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CompletionEvidences" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CompletionEvidences_ServiceRequests_ServiceRequestId" FOREIGN KEY ("ServiceRequestId") REFERENCES "ServiceRequests" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Payouts" (
    "Id" uuid NOT NULL,
    "ServiceRequestId" uuid NOT NULL,
    "WorkerId" uuid NOT NULL,
    "TotalAmount_Amount" numeric(18,2) NOT NULL,
    "TotalAmount_Currency" character varying(10) NOT NULL,
    "CommissionPercent" numeric(5,2) NOT NULL,
    "CommissionAmount_Amount" numeric(18,2) NOT NULL,
    "CommissionAmount_Currency" character varying(10) NOT NULL,
    "WorkerAmount_Amount" numeric(18,2) NOT NULL,
    "WorkerAmount_Currency" character varying(10) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "PaymentReference" text,
    CONSTRAINT "PK_Payouts" PRIMARY KEY ("Id")
);

CREATE TABLE "PriceAdjustmentRequests" (
    "Id" uuid NOT NULL,
    "ServiceRequestId" uuid NOT NULL,
    "OldPrice_Amount" numeric(18,2) NOT NULL,
    "OldPrice_Currency" character varying(10) NOT NULL,
    "NewPrice_Amount" numeric(18,2) NOT NULL,
    "NewPrice_Currency" character varying(10) NOT NULL,
    "Reason" character varying(1000) NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid NOT NULL,
    "ProcessedAt" timestamp with time zone,
    "ProcessedBy" uuid,
    CONSTRAINT "PK_PriceAdjustmentRequests" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_CommissionSettings_ServiceDefinitionId" ON "CommissionSettings" ("ServiceDefinitionId");

CREATE INDEX "IX_CompletionEvidences_ServiceRequestId" ON "CompletionEvidences" ("ServiceRequestId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260324163227_RedesignMasterFlow', '8.0.4');

COMMIT;

