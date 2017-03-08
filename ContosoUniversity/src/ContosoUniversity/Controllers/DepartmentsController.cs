using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;

namespace ContosoUniversity.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly SchoolContext _context;

        public DepartmentsController(SchoolContext context)
        {
            _context = context;    
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            var schoolContext = _context.Departments.Include(d => d.Administrator);
            return View(await schoolContext.ToListAsync());
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(i => i.Administrator)
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.DepartmentID == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            ViewData["InstructorID"] = new SelectList(_context.Instructors, "ID", "FirstName");
            return View();
        }

        // POST: Departments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartmentID,Budget,InstructorID,Name,RowVersion,StartDate")] Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["InstructorID"] = new SelectList(_context.Instructors, "ID", "FullName", department.InstructorID);
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(i => i.Administrator)
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.DepartmentID == id);
            if (department == null)
            {
                return NotFound();
            }
            ViewData["InstructorID"] = new SelectList(_context.Instructors, "ID", "FullName", department.InstructorID);
            return View(department);
        }

        // POST: Departments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, byte[] rowVersion)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.Include(i => i.Administrator).SingleOrDefaultAsync(m => m.DepartmentID == id);

            if(department == null)
            {
                Department deletedDepartment = new Department();
                await TryUpdateModelAsync(deletedDepartment);
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. the department was delated by another user.");
                ViewData["InstructorID"] = new SelectList(_context.Instructors, "ID", "FullName", deletedDepartment.InstructorID);
                return View(deletedDepartment);
            }

            _context.Entry(department).Property("RowVersion").OriginalValue = rowVersion;

            if (await TryUpdateModelAsync<Department>(department, "", s => s.Name, s => s.StartDate, s => s.Budget, s => s.InstructorID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var databaseEntity = await _context.Departments.AsNoTracking().SingleAsync(d => d.DepartmentID == ((Department)exceptionEntry.Entity).DepartmentID);
                    var databaseEntry = _context.Entry(databaseEntity);

                    var databaseName = (string)databaseEntry.Property("Name").CurrentValue;
                    var proposedName = (string)exceptionEntry.Property("Name").CurrentValue;
                    if(databaseName == proposedName)
                    {
                        ModelState.AddModelError("Name", $"Current value: {databaseName}");
                    }

                    var databaseBudget = (decimal)databaseEntry.Property("Budget").CurrentValue;
                    var proposedBudget = (decimal)exceptionEntry.Property("Budget").CurrentValue;
                    if(databaseBudget == proposedBudget)
                    {
                        ModelState.AddModelError("Budget", $"Current value: {databaseBudget:c}");
                    }

                    var databaseStartDate = (DateTime)databaseEntry.Property("StartDate").CurrentValue;
                    var proposedStartDate = (DateTime)exceptionEntry.Property("StartDate").CurrentValue;
                    if(databaseStartDate == proposedStartDate)
                    {
                        ModelState.AddModelError("StartDate", $"Current value: {databaseStartDate:d}");
                    }

                    var databaseInstructorID = (int)databaseEntry.Property("InstructorID").CurrentValue;
                    var proposedInstructorID = (int)exceptionEntry.Property("InstructorID").CurrentValue;
                    if(databaseInstructorID == proposedInstructorID)
                    {
                        Instructor databaseInstructor = await _context.Instructors.SingleAsync(i => i.ID == databaseInstructorID);
                        ModelState.AddModelError("InstructorID", $"Current value: {databaseInstructor.FullName}");
                    }

                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                        + "was modified by another user after you got the original value. "
                        + "the edit operation was canceled and the current values in the database "
                        + "have been displayed. if you still want to edit this record click "
                        + "the Save button again. otherwise click the Back to list hyperlink.");

                    department.RowVersion = (byte[])databaseEntry.Property("RowVersion").CurrentValue;
                    ModelState.Remove("RowVersion");

                }
            }
            ViewData["InstructorID"] = new SelectList(_context.Instructors, "ID", "FullName", department.InstructorID);
            return View(department);
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.Administrator)
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.DepartmentID == id);
            if (department == null)
            {
                if(concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction("Index");
                }
                return NotFound();
            }

            if(concurrencyError.GetValueOrDefault())
            {
                ModelState.AddModelError(string.Empty, "The record you attempted to delete "
                    + "was modified by another user after you got the original value. "
                    + "the delete operation was canceled and the current values in the database "
                    + "have been displayed. if you still want to delete this record click "
                    + "the Save button again. otherwise click the Back to list hyperlink.");

            }

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Department department)
        {
            try
            {
                if(await _context.Departments.AnyAsync(m => m.DepartmentID == department.DepartmentID))
                {
                    _context.Departments.Remove(department);
                    await _context.SaveChangesAsync();

                }
                return RedirectToAction("Index");
            }
            catch(DbUpdateConcurrencyException)
            {
                return RedirectToAction("Delete", new { concurrentcyError = true, id = department.DepartmentID });
            }
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.DepartmentID == id);
        }
    }
}
