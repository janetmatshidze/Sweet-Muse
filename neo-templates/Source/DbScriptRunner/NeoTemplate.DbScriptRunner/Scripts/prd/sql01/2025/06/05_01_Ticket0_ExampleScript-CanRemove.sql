/*
Summary: This is just an example script. You can remove this if you want.
It is named with prefix 05_01 meaning it was the 1st script on the 5th day of the month.

Timeout: 30 *you can override the command timeout for an individual script
*/

USE [NeoTemplate.DbUpdates]

-- Expected: 3  *you can specify the expected result row count here as a safety net.
-- The DbScriptRunner will inject print statements here to output the actual results and compare with this number, if they do not match it will rollback the transaction.
-- NOTE: Expected statements are not required

SELECT *
INTO #Test
FROM
(
  SELECT 'C' as Val1
  UNION ALL
  SELECT 'D'
  UNION ALL
  SELECT 'E'
) x

-- 2 rows
INSERT INTO #Test
SELECT 'A'
UNION ALL
SELECT 'B'

-- 1 row
INSERT INTO #Test
SELECT 'F'

-- 6 rows
DELETE FROM #Test

DROP TABLE #Test