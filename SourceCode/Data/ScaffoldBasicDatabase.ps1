Scaffold-DbContext Name=ConnectionStrings:TimetablePlanningDatabase Microsoft.EntityFrameworkCore.SqlServer -Force `
	-Context ModulesDbContext -Namespace ModulesRegistry.Data -OutputDir . `
	-ContextNamespace ModulesRegistry.Data -ContextDir . `
	-Tables Country, Group, GroupMember, User, Person, Scale, ModuleStandard, Module, ModuleOwnership, Cargo, OperatingDay, OperatingBasicDay, Operator,  Region, Station, StationTrack, StationCustomer, StationCustomerCargo, ExternalStation, ExternalStationCustomer, CargoDirection, CargoReadyTime, CargoRelation, CargoUnit, ExternalStationCustomerCargo 