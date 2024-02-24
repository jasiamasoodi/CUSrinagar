using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class EmployeeAttendance
    {
        public Guid Attendance_ID { get; set; }

        public Guid Employee_ID { get; set; }

        public DateTime AttendanceDate { get; set; }

        public short Status { get; set; }
    }

    public class EmployeeDepartment
    {
        public Guid Department_ID { get; set; }

        public string DepartmentName { get; set; }

        public string HOD { get; set; }

        public Guid Employee_ID { get; set; }
    }

    [Table("Employees")]
    public class Employee
    {
        public Guid Employee_ID { get; set; }

        
        public string EmployeeID { get; set; }

        [Required(ErrorMessage = "EmployeeFullName cannot be Empty")]
        public string EmployeeFullName { get; set; }

        [Required(ErrorMessage = "Phone cannot be Empty")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email cannot be Empty")]
        public string Email { get; set; }

        [IgnoreDBWriter]
        public string DepartmentName { get; set; }

        [IgnoreDBWriter]
        public string HOD { get; set; }

        public Guid Department_ID { get; set; }

        [Required(ErrorMessage = "DOB cannot be Empty")]
        [IgnoreDBRead, IgnoreDBWriter]
        public string EnteredDOB { get; set; }
                
                
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "Status cannot be Empty")]
        public EmployeeStatus Status { get; set; }

        [Required(ErrorMessage = "FathersName cannot be Empty")]
        public string FathersName { get; set; }

        
        public string MothersName { get; set; }

        [Required(ErrorMessage = "PermanentAddress cannot be Empty")]
        public string PermanentAddress { get; set; }

        public string PinCode { get; set; }

        [Required(ErrorMessage = "PresentAddress cannot be Empty")]
        public string PresentAddress { get; set; }

        [Required(ErrorMessage = "Designation cannot be Empty")]
        public string Designation { get; set; }

        
        public EmployeeType EmployeeType { get; set; }

        public bool ISNPS { get; set; }

        public Guid EmployeeAttendance_ID { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public List<EmployeeAttendance> Attendance { get; set; }

        [IgnoreDBRead, IgnoreDBWriter]
        public List<EmployeeDepartment> Departments { get; set; }
    }


}
