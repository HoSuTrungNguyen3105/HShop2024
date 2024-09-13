﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HShop2024.Data;
using HShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    // GET: NhanVien/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var khachHang = await _context.KhachHangs.FindAsync(id);
        if (khachHang == null)
        {
            return NotFound();
        }

        return View(khachHang);
    }

    // POST: Admin/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(List<KhachHang> KhachHangs)
    {
        if (ModelState.IsValid)
        {
            try
            {
                foreach (var khachHang in KhachHangs)
                {
                    var existingKhachHang = await _context.KhachHangs.FindAsync(khachHang.MaKh);
                    if (existingKhachHang != null)
                    {
                        existingKhachHang.HieuLuc = khachHang.HieuLuc;
                        _context.Update(existingKhachHang);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Xử lý lỗi nếu có
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật dữ liệu.");
            }
            return RedirectToAction(nameof(Index));
        }
        return View();
    }

    private bool KhachHangExists(string id)
    {
        return _context.KhachHangs.Any(e => e.MaKh == id);
    }
}
