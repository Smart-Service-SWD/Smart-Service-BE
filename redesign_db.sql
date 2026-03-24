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

