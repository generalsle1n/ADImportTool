using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ADImportTool.Model
{

    public class ManagerSettings
    {
        public bool Enabled { get; set; }
        public string ResolveManagerBy { get; set; }
    }

    public class Attributes
    {
        public Active Active { get; set; }
        public Mail Mail { get; set; }
        public FirstName FirstName { get; set; }
        public LastName LastName { get; set; }
        public FullName FullName { get; set; }
        public Manager Manager { get; set; }
        public UnresolvedManager UnresolvedManager { get; set; }
        public Position Position { get; set; }
        public CostCenter CostCenter { get; set; }
        public EmployeeNumber EmployeeNumber { get; set; }
        public TimecardNumber TimecardNumber { get; set; }
        public Salution Salution { get; set; }
        public Area Area { get; set; }
        public Department Department { get; set; }
        public Team Team { get; set; }
        public Intent Intent { get; set; }

    }
    public class AttributeSettings
    {
        public string Field { get; set; }
        public string Manipulation { get; set; }
        public string GenerateFrom { get; set; }
        public string Destination { get; set; }
    }

    public class Active
    {
        public AttributeSettings Settings { get; set; }
    }
    public class Mail
    {
        public AttributeSettings Settings { get; set; }
    }
    public class FirstName
    {
        public AttributeSettings Settings { get; set; }
    }
    public class LastName
    {
        public AttributeSettings Settings { get; set; }
    }
    public class FullName
    {
        public AttributeSettings Settings { get; set; }
    }
    public class Manager
    {
        public AttributeSettings Settings { get; set; }
    }
    public class UnresolvedManager
    {
        public AttributeSettings Settings { get; set; }
    }
    public class Position
    {
        public AttributeSettings Settings { get; set; }
    }
    public class CostCenter
    {
        public AttributeSettings Settings { get; set; }
    }
    public class EmployeeNumber
    {
        public AttributeSettings Settings { get; set; }
    }
    public class TimecardNumber
    {
        public AttributeSettings Settings { get; set; }
    }
    public class Salution
    {
        public AttributeSettings Settings { get; set; }
    }
    public class Area
    {
        public AttributeSettings Settings { get; set; }
    }
    public class Department
    {
        public AttributeSettings Settings { get; set; }
    }
    public class Team
    {
        public AttributeSettings Settings { get; set; }
    }
    public class Intent
    {
        public AttributeSettings Settings { get; set; }
    }
}
