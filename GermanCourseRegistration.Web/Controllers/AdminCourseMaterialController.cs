﻿using GermanCourseRegistration.EntityModels;
using GermanCourseRegistration.Web.Models.ViewModels;
using GermanCourseRegistration.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using GermanCourseRegistration.Web.Services;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace GermanCourseRegistration.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminCourseMaterialController : Controller
{
    private readonly ICourseMaterialRepository courseMaterialRepository;
    private readonly UserManager<IdentityUser> userManager;

    private const string AddAction = "Add";
    private const string EditAction = "Edit";

    public AdminCourseMaterialController(
        ICourseMaterialRepository courseMaterialRepository,
        UserManager<IdentityUser> userManager)
    {
        this.courseMaterialRepository = courseMaterialRepository;
        this.userManager = userManager;
    }

    //
    // Reading Method
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var courseMaterials = Enumerable.Empty<CourseMaterial>();

        try
        {
            courseMaterials = await courseMaterialRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            WriteLine(ex.Message);
            WriteLine(ex.StackTrace);
        }

        if (!courseMaterials.Any())
        {
            // Show error notification
            View(Enumerable.Empty<CourseMaterial>());
        }

        var models = new List<CourseMaterialView>();

        foreach (var courseMaterial in courseMaterials)
        {
            models.Add(MapCourseMaterialToViewModel(courseMaterial));
        }

        return View(models);
    }

    //
    // Writing Methods
    [HttpGet]
    public async Task<IActionResult> Add()
    {
        var model = new CourseMaterialView();

        // Load the categories
        var categories = new List<string>()
        {
            CourseMaterial.PhysicalCopyCategory, 
            CourseMaterial.DigitalCopyCategory,
            CourseMaterial.AudioCopyCategory
        };
        model.AvailableCategories = categories.Select(c => new SelectListItem
        {
            Text = c,
            Value = c
        });

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Add(CourseMaterialView model)
    {
        // Get Id of logged in user
        Guid loginId = await UserAccountService.GetCurrentUserId(userManager, User);
        var courseMaterial = MapViewModelToCourseMaterial(model, loginId, AddAction);
      
        bool isAdded = false;

        try
        {
            isAdded = await courseMaterialRepository.AddAsync(courseMaterial);
        }
        catch (Exception ex)
        {
            WriteLine(ex.Message);
            WriteLine(ex.StackTrace);
        }

        if (isAdded)
        {
            // Show success notification
            return RedirectToAction("List");
        }

        // Show error notification
        return RedirectToAction("List");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        CourseMaterial? courseMaterial = null;

        try
        {
            courseMaterial = await courseMaterialRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            WriteLine(ex.Message);
            WriteLine(ex.StackTrace);
        }

        if (courseMaterial == null)
        {
            // Show error notification
            return View("Error");
        }

        var model = MapCourseMaterialToViewModel(courseMaterial);

        // Load the categories, and assign the id
        var categories = new List<string>()
        {
            CourseMaterial.PhysicalCopyCategory,
            CourseMaterial.DigitalCopyCategory,
            CourseMaterial.AudioCopyCategory
        };
        model.AvailableCategories = categories.Select(c => new SelectListItem
        {
            Text = c,
            Value = c
        });

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CourseMaterialView model)
    {
        Guid loginId = await UserAccountService.GetCurrentUserId(userManager, User);
        var courseMaterial = MapViewModelToCourseMaterial(model, loginId, EditAction);

        CourseMaterial? updatedCourseMaterial = null;

        try
        {
            updatedCourseMaterial = await courseMaterialRepository.UpdateAsync(courseMaterial);
        }
        catch (Exception ex)
        {
            WriteLine(ex.Message);
            WriteLine(ex.StackTrace);
        }

        if (updatedCourseMaterial != null)
        {
            // Show success notification
            return RedirectToAction("List");
        }

        // Show error notification
        return RedirectToAction("List");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        CourseMaterial? deletedCourseMaterial = null;

        try
        {
            deletedCourseMaterial = await courseMaterialRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            WriteLine(ex.Message);
            WriteLine(ex.StackTrace);
        }

        if (deletedCourseMaterial != null)
        {
            // Show success notification
            return RedirectToAction("List");
        }

        // Show error notification
        return RedirectToAction("List", new { id });
    }

    //
    // Mapping Methods
    private CourseMaterial MapViewModelToCourseMaterial(
        CourseMaterialView model, Guid loginId, string action)
    {
        if (action == AddAction)
        {
            return new CourseMaterial
            {
                Name = model.Name,
                Category = model.Category,
                Description = model.Description,
                Price = model.Price,
                CreatedBy = loginId,
                CreatedOn = DateTime.Now
            };
        }
        else if (action == EditAction)
        {
            return new CourseMaterial
            {
                Id = model.Id,
                Name = model.Name,
                Category = model.Category,
                Description = model.Description,
                Price = model.Price,
                LastModifiedBy = loginId,
                LastModifiedOn = DateTime.Now
            };
        }
        else
        {
            return new();
        }
    }

    private CourseMaterialView MapCourseMaterialToViewModel(CourseMaterial model)
    {
        return new CourseMaterialView
        {
            Id = model.Id,
            Name = model.Name,
            Category = model.Category,
            Description = model.Description,
            Price = Convert.ToDecimal(model.Price.ToString("0.####"))
        };
    }
}
