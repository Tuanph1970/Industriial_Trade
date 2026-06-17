-- Extensions used by the design (PostGIS for spatial data; ltree for the org-unit tree path;
-- pg_trgm for keyword search). Schemas themselves are created by EF Core migrations.
CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS ltree;
CREATE EXTENSION IF NOT EXISTS pg_trgm;
