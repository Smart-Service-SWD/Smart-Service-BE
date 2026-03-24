-- SCRIPT DỌN DẸP DỮ LIỆU TEST (CHỈ XÓA ĐƠN HÀNG VÀ CÁC THỨ LIÊN QUAN)
-- Cẩn thận: Giữ lại Users, ServiceDefinitions, Categories, v.v.

BEGIN;

-- 1. Xóa dữ liệu liên quan đến đơn hàng (Các bảng con)
DELETE FROM "ServiceFeedbacks";
DELETE FROM "PriceAdjustmentRequests";
DELETE FROM "CompletionEvidences";
DELETE FROM "Assignments";
DELETE FROM "MatchingResults";
DELETE FROM "ServiceAnalyses";
DELETE FROM "ServiceAttachments";
DELETE FROM "ActivityLogs";
DELETE FROM "Payouts";

-- 2. Xóa bảng đơn hàng chính
DELETE FROM "ServiceRequests";

COMMIT;

-- THÔNG BÁO: Đã dọn dẹp sạch các yêu cầu dịch vụ. 
-- Các bảng cấu hình hệ thống và tài khoản người dùng vẫn được giữ nguyên.
