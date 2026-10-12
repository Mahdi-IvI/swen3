BEGIN;

ALTER TABLE public."Documents"
    ADD COLUMN IF NOT EXISTS "SizeBytes" bigint;

COMMIT;
