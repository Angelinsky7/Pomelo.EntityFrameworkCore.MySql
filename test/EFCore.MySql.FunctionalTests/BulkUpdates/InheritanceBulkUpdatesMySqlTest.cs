using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.TestUtilities;
using MySqlConnector;
using Xunit;

namespace Pomelo.EntityFrameworkCore.MySql.FunctionalTests.BulkUpdates;

public class InheritanceBulkUpdatesMySqlTest : InheritanceBulkUpdatesTestBase<InheritanceBulkUpdatesMySqlFixture>
{
    public InheritanceBulkUpdatesMySqlTest(InheritanceBulkUpdatesMySqlFixture fixture)
        : base(fixture)
    {
        ClearLog();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_where_hierarchy(bool async)
    {
        await base.Delete_where_hierarchy(async);

        AssertSql(
"""
DELETE `a`
FROM `Animals` AS `a`
WHERE `a`.`Name` = 'Great spotted kiwi'
""");
    }

    public override async Task Delete_where_hierarchy_derived(bool async)
    {
        await base.Delete_where_hierarchy_derived(async);

        AssertSql(
"""
DELETE `a`
FROM `Animals` AS `a`
WHERE (`a`.`Discriminator` = 'Kiwi') AND (`a`.`Name` = 'Great spotted kiwi')
""");
    }

    public override async Task Delete_where_using_hierarchy(bool async)
    {
        await base.Delete_where_using_hierarchy(async);

        AssertSql(
"""
DELETE `c`
FROM `Countries` AS `c`
WHERE (
    SELECT COUNT(*)
    FROM `Animals` AS `a`
    WHERE (`c`.`Id` = `a`.`CountryId`) AND (`a`.`CountryId` > 0)) > 0
""");
    }

    public override async Task Delete_where_using_hierarchy_derived(bool async)
    {
        await base.Delete_where_using_hierarchy_derived(async);

        AssertSql(
"""
DELETE `c`
FROM `Countries` AS `c`
WHERE (
    SELECT COUNT(*)
    FROM `Animals` AS `a`
    WHERE ((`c`.`Id` = `a`.`CountryId`) AND (`a`.`Discriminator` = 'Kiwi')) AND (`a`.`CountryId` > 0)) > 0
""");
    }

    public override async Task Delete_GroupBy_Where_Select_First(bool async)
    {
        await base.Delete_GroupBy_Where_Select_First(async);

        AssertSql();
    }

    public override async Task Delete_GroupBy_Where_Select_First_2(bool async)
    {
        await base.Delete_GroupBy_Where_Select_First_2(async);

        AssertSql();
    }

    public override async Task Delete_GroupBy_Where_Select_First_3(bool async)
    {
        // Not supported by MySQL:
        //     Error Code: 1093. You can't specify target table 'c' for update in FROM clause
        await Assert.ThrowsAsync<MySqlException>(
            () => base.Delete_GroupBy_Where_Select_First_3(async));

        AssertSql(
"""
DELETE `a`
FROM `Animals` AS `a`
WHERE EXISTS (
    SELECT 1
    FROM `Animals` AS `a0`
    GROUP BY `a0`.`CountryId`
    HAVING (COUNT(*) < 3) AND ((
        SELECT `a1`.`Id`
        FROM `Animals` AS `a1`
        WHERE `a0`.`CountryId` = `a1`.`CountryId`
        LIMIT 1) = `a`.`Id`))
""");
    }

    public override async Task Delete_where_keyless_entity_mapped_to_sql_query(bool async)
    {
        await base.Delete_where_keyless_entity_mapped_to_sql_query(async);

        AssertSql();
    }

    public override async Task Delete_where_hierarchy_subquery(bool async)
    {
        await base.Delete_where_hierarchy_subquery(async);

        AssertSql(
"""
@__p_1='3'
@__p_0='0'

DELETE `a`
FROM `Animals` AS `a`
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT `a0`.`Id`, `a0`.`CountryId`, `a0`.`Discriminator`, `a0`.`Name`, `a0`.`Species`, `a0`.`EagleId`, `a0`.`IsFlightless`, `a0`.`Group`, `a0`.`FoundOn`
        FROM `Animals` AS `a0`
        WHERE `a0`.`Name` = 'Great spotted kiwi'
        ORDER BY `a0`.`Name`
        LIMIT @__p_1 OFFSET @__p_0
    ) AS `t`
    WHERE `t`.`Id` = `a`.`Id`)
""");
    }

    public override async Task Update_where_hierarchy(bool async)
    {
        await base.Update_where_hierarchy(async);

        AssertExecuteUpdateSql(
"""
UPDATE `Animals` AS `a`
SET `a`.`Name` = 'Animal'
WHERE `a`.`Name` = 'Great spotted kiwi'
""");
    }

    public override async Task Update_where_hierarchy_subquery(bool async)
    {
        await base.Update_where_hierarchy_subquery(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_where_hierarchy_derived(bool async)
    {
        await base.Update_where_hierarchy_derived(async);

        AssertExecuteUpdateSql(
"""
UPDATE `Animals` AS `a`
SET `a`.`Name` = 'Kiwi'
WHERE (`a`.`Discriminator` = 'Kiwi') AND (`a`.`Name` = 'Great spotted kiwi')
""");
    }

    public override async Task Update_where_using_hierarchy(bool async)
    {
        await base.Update_where_using_hierarchy(async);

        AssertExecuteUpdateSql(
"""
UPDATE `Countries` AS `c`
SET `c`.`Name` = 'Monovia'
WHERE (
    SELECT COUNT(*)
    FROM `Animals` AS `a`
    WHERE (`c`.`Id` = `a`.`CountryId`) AND (`a`.`CountryId` > 0)) > 0
""");
    }

    public override async Task Update_where_using_hierarchy_derived(bool async)
    {
        await base.Update_where_using_hierarchy_derived(async);

        AssertExecuteUpdateSql(
"""
UPDATE `Countries` AS `c`
SET `c`.`Name` = 'Monovia'
WHERE (
    SELECT COUNT(*)
    FROM `Animals` AS `a`
    WHERE ((`c`.`Id` = `a`.`CountryId`) AND (`a`.`Discriminator` = 'Kiwi')) AND (`a`.`CountryId` > 0)) > 0
""");
    }

    public override async Task Update_where_keyless_entity_mapped_to_sql_query(bool async)
    {
        await base.Update_where_keyless_entity_mapped_to_sql_query(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_with_interface_in_property_expression(bool async)
    {
        await base.Update_with_interface_in_property_expression(async);

        AssertSql(
"""
SELECT `d`.`Id`, `d`.`Discriminator`, `d`.`SortIndex`, `d`.`CaffeineGrams`, `d`.`CokeCO2`, `d`.`SugarGrams`
FROM `Drinks` AS `d`
WHERE `d`.`Discriminator` = 'Coke'
""",
                //
                """
UPDATE `Drinks` AS `d`
SET `d`.`SugarGrams` = 0
WHERE `d`.`Discriminator` = 'Coke'
""",
                //
                """
SELECT `d`.`Id`, `d`.`Discriminator`, `d`.`SortIndex`, `d`.`CaffeineGrams`, `d`.`CokeCO2`, `d`.`SugarGrams`
FROM `Drinks` AS `d`
WHERE `d`.`Discriminator` = 'Coke'
""");
    }

    public override async Task Update_with_interface_in_EF_Property_in_property_expression(bool async)
    {
        await base.Update_with_interface_in_EF_Property_in_property_expression(async);

        AssertSql(
"""
SELECT `d`.`Id`, `d`.`Discriminator`, `d`.`SortIndex`, `d`.`CaffeineGrams`, `d`.`CokeCO2`, `d`.`SugarGrams`
FROM `Drinks` AS `d`
WHERE `d`.`Discriminator` = 'Coke'
""",
                //
                """
UPDATE `Drinks` AS `d`
SET `d`.`SugarGrams` = 0
WHERE `d`.`Discriminator` = 'Coke'
""",
                //
                """
SELECT `d`.`Id`, `d`.`Discriminator`, `d`.`SortIndex`, `d`.`CaffeineGrams`, `d`.`CokeCO2`, `d`.`SugarGrams`
FROM `Drinks` AS `d`
WHERE `d`.`Discriminator` = 'Coke'
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}