using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HShop2024.Data;
using HShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HShop2024.Controllers;

public class AdminController : Controller
{
    private readonly Hshop2023Context _context;

    public AdminController(Hshop2023Context context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        List<KhachHang> employees = _context.KhachHangs.ToList();
        return View(employees);
    }

    public IActionResult Create()
    {
        return View();
    }


    public IActionResult Create(string id)
    {
        string id2 = id;
        KhachHang khachHang = _context.KhachHangs.SingleOrDefault((KhachHang kh) => kh.MaKh == id2);
        if (khachHang == null)
        {
            return NotFound();
        }
        _context.KhachHangs.Add(khachHang);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Edit(int id)
    {
        KhachHang employee = _context.KhachHangs.Find(id);
        if (employee == null)
        {
            return NotFound();
        }
        return View(employee);
    }

    [HttpPost]
    public IActionResult Edit(KhachHang model)
    {
        if (base.ModelState.IsValid)
        {
            _context.KhachHangs.Update(model);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(model);
    }

    public IActionResult Delete(string id)
    {
        string id2 = id;
        KhachHang khachHang = _context.KhachHangs.SingleOrDefault((KhachHang kh) => kh.MaKh == id2);
        if (khachHang == null)
        {
            return NotFound();
        }
        _context.KhachHangs.Remove(khachHang);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}
