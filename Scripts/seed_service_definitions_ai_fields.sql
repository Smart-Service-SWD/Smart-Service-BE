-- ============================================================
-- Seed Script: Set complexity_range + is_dangerous for existing ServiceDefinitions
-- Run once after migration AddAiAnalysisFields is applied
-- All values are defaults; admins can adjust via API later
-- ============================================================

-- ── Điện / Electrical ───────────────────────────────────────
-- Dangerous by nature → is_dangerous = true
UPDATE service_definition
SET
    complexity_range = ARRAY[3, 5],
    is_dangerous     = true
WHERE
    is_active = true
    AND (
        name ILIKE '%điện 3 pha%'
        OR name ILIKE '%3 pha%'
        OR name ILIKE '%cân pha%'
        OR name ILIKE '%tủ điện%'
        OR name ILIKE '%cháy%'
    );

-- General electrical – moderate danger
UPDATE service_definition
SET
    complexity_range = ARRAY[2, 4],
    is_dangerous     = true
WHERE
    is_active = true
    AND (
        name ILIKE '%điện%'
        OR name ILIKE '%aptomat%'
        OR name ILIKE '%ổ cắm%'
        OR name ILIKE '%dây điện%'
    )
    AND is_dangerous = false;  -- don't override already-set rows

-- ── Ống nước / Plumbing ─────────────────────────────────────
UPDATE service_definition
SET
    complexity_range = ARRAY[1, 3],
    is_dangerous     = false
WHERE
    is_active = true
    AND (
        name ILIKE '%ống nước%'
        OR name ILIKE '%vòi nước%'
        OR name ILIKE '%bồn rửa%'
        OR name ILIKE '%cống%'
    )
    AND is_dangerous = false;

-- ── Gas – always dangerous ──────────────────────────────────
UPDATE service_definition
SET
    complexity_range = ARRAY[3, 5],
    is_dangerous     = true
WHERE
    is_active = true
    AND (
        name ILIKE '%gas%'
        OR name ILIKE '%bếp gas%'
        OR name ILIKE '%bình gas%'
    );

-- ── IT / Máy tính ───────────────────────────────────────────
UPDATE service_definition
SET
    complexity_range = ARRAY[1, 3],
    is_dangerous     = false
WHERE
    is_active = true
    AND (
        name ILIKE '%máy tính%'
        OR name ILIKE '%laptop%'
        OR name ILIKE '%máy chủ%'
        OR name ILIKE '%hệ điều hành%'
        OR name ILIKE '%phần mềm%'
        OR name ILIKE '%mạng%'
    )
    AND is_dangerous = false;

-- ── Gia dụng / Home appliances ─────────────────────────────
UPDATE service_definition
SET
    complexity_range = ARRAY[1, 3],
    is_dangerous     = false
WHERE
    is_active = true
    AND (
        name ILIKE '%điều hòa%'
        OR name ILIKE '%tủ lạnh%'
        OR name ILIKE '%máy giặt%'
        OR name ILIKE '%lò vi sóng%'
        OR name ILIKE '%quạt%'
    )
    AND is_dangerous = false;

-- ── Pháp lý / Legal ─────────────────────────────────────────
UPDATE service_definition
SET
    complexity_range = ARRAY[2, 4],
    is_dangerous     = false
WHERE
    is_active = true
    AND (
        name ILIKE '%luật%'
        OR name ILIKE '%pháp lý%'
        OR name ILIKE '%hợp đồng%'
        OR name ILIKE '%thừa kế%'
    )
    AND is_dangerous = false;

-- ── Fallback: set defaults for any remaining unset rows ─────
UPDATE service_definition
SET
    complexity_range = ARRAY[1, 3],
    is_dangerous     = false
WHERE
    is_active = true
    AND complexity_range = ARRAY[1, 3]  -- still at default from migration
    AND is_dangerous = false;
    -- (this is a no-op but documents intent)

-- Verify results
SELECT
    name,
    complexity_range,
    is_dangerous
FROM service_definition
WHERE is_active = true
ORDER BY is_dangerous DESC, name;
