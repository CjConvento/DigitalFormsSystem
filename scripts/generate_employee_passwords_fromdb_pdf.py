#!/usr/bin/env python3
"""
Generate PDF with employee credentials directly from the database.
Reads PlainTextPassword from the Employees table.
"""

import pyodbc
import os
from datetime import datetime
from reportlab.lib import colors
from reportlab.lib.pagesizes import A4, landscape
from reportlab.lib.units import inch, mm
from reportlab.platypus import SimpleDocTemplate, Table, TableStyle, Paragraph, Spacer
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.enums import TA_CENTER, TA_LEFT, TA_RIGHT

# ======================= CONFIGURATION =======================
SERVER = 'localhost\\SQLEXPRESS01'
DATABASE = 'DigitalFormsSystem'
OUTPUT_FILE = 'employee_credentials_from_db.pdf'
MANAGER_ID = '778'  # Gilbert Pesidas ID
# =============================================================

def get_employees():
    """Fetch employees from database with their plain text passwords."""
    try:
        # ✅ Use the exact connection string format from SSMS
        # with Encrypt and TrustServerCertificate for secure connection
        conn_str = (
            f'DRIVER={{ODBC Driver 18 for SQL Server}};'
            f'SERVER={SERVER};'
            f'DATABASE={DATABASE};'
            f'Trusted_Connection=yes;'
            f'Encrypt=yes;'
            f'TrustServerCertificate=yes;'
        )
        
        print("🔌 Attempting to connect with Driver 18...")
        conn = pyodbc.connect(conn_str, timeout=10)
        print("✅ Connected successfully using ODBC Driver 18")
        
        cursor = conn.cursor()
        
        query = """
            SELECT 
                ID,
                EmployeeNo,
                Name,
                Department,
                Location,
                Section,
                PlainTextPassword
            FROM Employees
            WHERE IsActive = 1
              AND PlainTextPassword IS NOT NULL
              AND PlainTextPassword != ''
            ORDER BY EmployeeNo
        """
        
        cursor.execute(query)
        rows = cursor.fetchall()
        
        employees = []
        for row in rows:
            emp = {
                'ID': str(row[0]),
                'EmployeeNo': row[1] if row[1] is not None else '',
                'Name': row[2] if row[2] is not None else '',
                'Department': row[3] if row[3] is not None else '',
                'Location': row[4] if row[4] is not None else '',
                'Section': row[5] if row[5] is not None else '',
                'PlainTextPassword': row[6] if row[6] is not None else 'N/A'
            }
            employees.append(emp)
        
        cursor.close()
        conn.close()
        
        print(f"✅ Found {len(employees)} employees with passwords")
        return employees
        
    except pyodbc.Error as e:
        print(f"❌ Database error: {e}")
        print("\n💡 Make sure:")
        print("  1. SQL Server is running")
        print("  2. Database name is correct: 'DigitalFormsSystem'")
        print("  3. You have permission to access the database")
        print("  4. ODBC Driver 18 is installed (or try Driver 17)")
        return []
    except Exception as e:
        print(f"❌ Unexpected error: {e}")
        return []

def create_pdf(employees, output_file):
    """Generate PDF with employee credentials table."""
    
    if not employees:
        print("❌ No employees found. Check the database connection.")
        return False
    
    # Create PDF document in landscape mode
    doc = SimpleDocTemplate(
        output_file,
        pagesize=landscape(A4),
        rightMargin=10*mm,
        leftMargin=10*mm,
        topMargin=15*mm,
        bottomMargin=15*mm
    )
    
    # Styles
    styles = getSampleStyleSheet()
    
    # Custom styles
    title_style = ParagraphStyle(
        'TitleStyle',
        parent=styles['Heading1'],
        fontSize=16,
        alignment=TA_CENTER,
        spaceAfter=6,
        textColor=colors.HexColor('#003366')
    )
    
    subtitle_style = ParagraphStyle(
        'SubtitleStyle',
        parent=styles['Heading2'],
        fontSize=12,
        alignment=TA_CENTER,
        spaceAfter=4,
        textColor=colors.HexColor('#003366')
    )
    
    warning_style = ParagraphStyle(
        'WarningStyle',
        parent=styles['Normal'],
        fontSize=10,
        alignment=TA_CENTER,
        textColor=colors.red,
        spaceAfter=8
    )
    
    header_style = ParagraphStyle(
        'HeaderStyle',
        parent=styles['Normal'],
        fontSize=8,
        alignment=TA_CENTER,
        textColor=colors.white,
        fontName='Helvetica'
    )
    
    cell_style = ParagraphStyle(
        'CellStyle',
        parent=styles['Normal'],
        fontSize=7,
        alignment=TA_LEFT,
        fontName='Helvetica'
    )
    
    cell_center_style = ParagraphStyle(
        'CellCenterStyle',
        parent=styles['Normal'],
        fontSize=7,
        alignment=TA_CENTER,
        fontName='Helvetica'
    )
    
    cell_password_style = ParagraphStyle(
        'CellPasswordStyle',
        parent=styles['Normal'],
        fontSize=7,
        alignment=TA_CENTER,
        fontName='Helvetica',
        textColor=colors.HexColor('#006600')
    )
    
    cell_manager_style = ParagraphStyle(
        'CellManagerStyle',
        parent=styles['Normal'],
        fontSize=7,
        alignment=TA_CENTER,
        fontName='Helvetica',
        textColor=colors.HexColor('#CC0000')
    )
    
    # Build document elements
    elements = []
    
    # ============ HEADER ============
    elements.append(Paragraph("HST DIGITAL FORMS SYSTEM", title_style))
    elements.append(Paragraph("EMPLOYEE CREDENTIALS REPORT", subtitle_style))
    elements.append(Paragraph("⚠️ FOR DEMO / TESTING PURPOSES ONLY ⚠️", warning_style))
    
    # Date and time
    date_style = ParagraphStyle(
        'DateStyle',
        parent=styles['Normal'],
        fontSize=8,
        alignment=TA_RIGHT,
        textColor=colors.grey
    )
    elements.append(Paragraph(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}", date_style))
    elements.append(Spacer(1, 6))
    
    # ============ TABLE DATA ============
    table_data = []
    
    # Header row
    header_row = [
        Paragraph("<b>ID</b>", header_style),
        Paragraph("<b>Employee No.</b>", header_style),
        Paragraph("<b>Employee Name</b>", header_style),
        Paragraph("<b>Department</b>", header_style),
        Paragraph("<b>Location</b>", header_style),
        Paragraph("<b>Section</b>", header_style),
        Paragraph("<b>Default Password</b>", header_style),
    ]
    table_data.append(header_row)
    
    # Data rows
    for emp in employees:
        employee_no = emp['EmployeeNo']
        name = emp['Name']
        department = emp['Department']
        location = emp['Location']
        section = emp['Section']
        emp_id = emp['ID']
        default_password = emp['PlainTextPassword']
        
        is_manager = (emp_id == MANAGER_ID)
        
        id_cell = Paragraph(emp_id, cell_center_style)
        emp_no_cell = Paragraph(employee_no, cell_center_style)
        name_cell = Paragraph(name, cell_style)
        dept_cell = Paragraph(department, cell_center_style)
        loc_cell = Paragraph(location, cell_center_style)
        section_cell = Paragraph(section, cell_style)
        
        if is_manager:
            password_cell = Paragraph(
                f'<b><font color="#CC0000">⭐ {default_password}</font></b>',
                cell_manager_style
            )
        else:
            password_cell = Paragraph(
                f'<font color="#006600">{default_password}</font>',
                cell_password_style
            )
        
        row = [id_cell, emp_no_cell, name_cell, dept_cell, loc_cell, section_cell, password_cell]
        table_data.append(row)
    
    # ============ CREATE TABLE ============
    col_widths = [
        0.8 * inch,   # ID
        1.4 * inch,   # Employee No.
        2.2 * inch,   # Name
        1.2 * inch,   # Department
        0.8 * inch,   # Location
        1.6 * inch,   # Section
        2.0 * inch,   # Default Password
    ]
    
    total_width = sum(col_widths)
    page_width = landscape(A4)[0] - 20*mm
    scale_factor = page_width / total_width
    col_widths = [w * scale_factor for w in col_widths]
    
    table = Table(table_data, colWidths=col_widths, repeatRows=1)
    
    # ============ TABLE STYLES ============
    style = TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.HexColor('#003366')),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.white),
        ('ALIGN', (0, 0), (-1, 0), 'CENTER'),
        ('FONTSIZE', (0, 0), (-1, 0), 8),
        ('BOLD', (0, 0), (-1, 0), 1),
        ('GRID', (0, 0), (-1, -1), 0.5, colors.grey),
        ('TOPPADDING', (0, 0), (-1, -1), 3),
        ('BOTTOMPADDING', (0, 0), (-1, -1), 3),
        ('LEFTPADDING', (0, 0), (-1, -1), 4),
        ('RIGHTPADDING', (0, 0), (-1, -1), 4),
        ('ROWBACKGROUNDS', (0, 1), (-1, -1), [colors.whitesmoke, colors.white]),
    ])
    
    # Highlight Gilbert's row
    for i, emp in enumerate(employees):
        if emp['ID'] == MANAGER_ID:
            row_index = i + 1
            style.add('BACKGROUND', (0, row_index), (-1, row_index), colors.HexColor('#FFF3CD'))
            style.add('TEXTCOLOR', (6, row_index), (6, row_index), colors.HexColor('#CC0000'))
            style.add('BOLD', (6, row_index), (6, row_index), 1)
            style.add('BOX', (0, row_index), (-1, row_index), 1.5, colors.HexColor('#FF6B6B'))
            break
    
    table.setStyle(style)
    elements.append(table)
    
    # ============ LEGEND ============
    elements.append(Spacer(1, 8))
    
    legend_style = ParagraphStyle(
        'LegendStyle',
        parent=styles['Normal'],
        fontSize=9,
        alignment=TA_LEFT,
        fontName='Helvetica'
    )
    
    legend_text = """
    <font color="#CC0000">⭐</font> <b>Gilbert Pesidas (ID 778)</b> — <i>System Administrator / IT Manager</i>
    """
    elements.append(Paragraph(legend_text, legend_style))
    
    # ============ SUMMARY ============
    elements.append(Spacer(1, 4))
    
    total_employees = len(employees)
    summary_style = ParagraphStyle(
        'SummaryStyle',
        parent=styles['Normal'],
        fontSize=9,
        alignment=TA_LEFT,
        textColor=colors.HexColor('#003366')
    )
    
    elements.append(Paragraph(
        f"<b>Total Active Employees:</b> {total_employees}",
        summary_style
    ))
    
    # ============ FOOTER ============
    footer_style = ParagraphStyle(
        'FooterStyle',
        parent=styles['Normal'],
        fontSize=7,
        alignment=TA_CENTER,
        textColor=colors.grey
    )
    elements.append(Spacer(1, 4))
    elements.append(Paragraph(
        "This document contains confidential information. For authorized use only.",
        footer_style
    ))
    
    # ============ BUILD PDF ============
    try:
        doc.build(elements)
        print(f"✅ PDF generated successfully!")
        print(f"📄 Output file: {output_file}")
        print(f"📊 Total employees: {total_employees}")
        return True
    except Exception as e:
        print(f"❌ Error generating PDF: {e}")
        return False

def main():
    print("\n" + "="*60)
    print("📋 EMPLOYEE CREDENTIALS PDF GENERATOR")
    print("="*60)
    print(f"📡 Connecting to: {SERVER}\\{DATABASE}")
    print()
    
    employees = get_employees()
    
    if not employees:
        print("❌ No employees found. Make sure:")
        print("   - SQL Server is running")
        print("   - Database 'DigitalFormsSystem' exists")
        print("   - Employees have PlainTextPassword values")
        return
    
    print(f"\n👥 Found {len(employees)} employees with passwords")
    
    # Show sample
    print("\n📋 Sample passwords (first 5):")
    for emp in employees[:5]:
        star = "⭐ " if emp['ID'] == MANAGER_ID else "  "
        print(f"  {star}{emp['EmployeeNo']:15} → {emp['PlainTextPassword']}")
    
    print()
    
    success = create_pdf(employees, OUTPUT_FILE)
    
    if success:
        print(f"\n✅ Done! PDF saved as: {OUTPUT_FILE}")
        print(f"📂 Location: {os.path.abspath(OUTPUT_FILE)}")
    else:
        print("\n❌ PDF generation failed.")

if __name__ == "__main__":
    main()