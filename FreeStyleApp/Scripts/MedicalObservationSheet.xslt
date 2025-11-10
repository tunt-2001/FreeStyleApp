<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html" encoding="UTF-8" indent="yes"/>
    
    <xsl:template match="/">
        <html>
            <head>
                <meta charset="UTF-8"/>
                <title>Phiếu Theo Dõi và Chăm Sóc (Cấp 1)</title>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        font-size: 11px;
                        margin: 10px;
                        padding: 0;
                    }
                    .header {
                        text-align: center;
                        margin-bottom: 15px;
                    }
                    .header h2 {
                        margin: 5px 0;
                        font-size: 14px;
                        font-weight: bold;
                        text-transform: uppercase;
                    }
                    .info-section {
                        margin-bottom: 10px;
                    }
                    .info-row {
                        display: flex;
                        margin-bottom: 3px;
                    }
                    .info-label {
                        font-weight: bold;
                        min-width: 120px;
                    }
                    .info-value {
                        flex: 1;
                    }
                    table {
                        width: 100%;
                        border-collapse: collapse;
                        margin-top: 10px;
                        font-size: 10px;
                    }
                    th, td {
                        border: 1px solid #000;
                        padding: 3px;
                        text-align: center;
                        vertical-align: middle;
                    }
                    th {
                        background-color: #f0f0f0;
                        font-weight: bold;
                        font-size: 9px;
                    }
                    .row-label {
                        text-align: left;
                        font-weight: bold;
                        background-color: #e8e8e8;
                        min-width: 150px;
                    }
                    .date-time-header {
                        writing-mode: vertical-rl;
                        text-orientation: mixed;
                        height: 100px;
                        min-width: 30px;
                    }
                    .signature-section {
                        margin-top: 20px;
                        display: flex;
                        justify-content: space-between;
                    }
                    .signature-box {
                        text-align: center;
                        width: 200px;
                    }
                </style>
            </head>
            <body>
                <xsl:apply-templates select="TONG_HOP_PHIEU_CHAM_SOC1_DIEU_DUONG_NAYHCT"/>
            </body>
        </html>
    </xsl:template>
    
    <xsl:template match="TONG_HOP_PHIEU_CHAM_SOC1_DIEU_DUONG_NAYHCT">
        <xsl:for-each select="PHIEU_CHAM_SOC1_DIEU_DUONG_NAYHCT">
            <div class="sheet">
                <xsl:apply-templates select="Item"/>
            </div>
        </xsl:for-each>
    </xsl:template>
    
    <xsl:template match="Item">
        <xsl:variable name="ttChung" select="PageCls/TtChung"/>
        <xsl:variable name="pages" select="PageCls/Pages/Page"/>
        
        <!-- Header -->
        <div class="header">
            <h2>PHIẾU THEO DÕI VÀ CHĂM SÓC (Cấp 1)</h2>
        </div>
        
        <!-- Patient Information -->
        <div class="info-section">
            <div class="info-row">
                <span class="info-label">Đơn vị chủ quản:</span>
                <span class="info-value"><xsl:value-of select="$ttChung/DonViChuQuan"/></span>
            </div>
            <div class="info-row">
                <span class="info-label">Bệnh viện:</span>
                <span class="info-value"><xsl:value-of select="$ttChung/BenhVienTen"/></span>
            </div>
            <div class="info-row">
                <span class="info-label">Số vào viện:</span>
                <span class="info-value"><xsl:value-of select="$ttChung/SoVaoVien"/></span>
                <span class="info-label" style="margin-left: 20px;">Tên BN:</span>
                <span class="info-value"><xsl:value-of select="$ttChung/TenBN"/></span>
                <span class="info-label" style="margin-left: 20px;">Mã BN:</span>
                <span class="info-value"><xsl:value-of select="$ttChung/MaBN"/></span>
            </div>
            <div class="info-row">
                <span class="info-label">Tuổi:</span>
                <span class="info-value"><xsl:value-of select="$ttChung/Tuoi"/></span>
                <span class="info-label" style="margin-left: 20px;">Giới tính:</span>
                <span class="info-value"><xsl:value-of select="$ttChung/GioiTinh"/></span>
                <span class="info-label" style="margin-left: 20px;">Địa chỉ:</span>
                <span class="info-value"><xsl:value-of select="$ttChung/DiaChi"/></span>
            </div>
            <div class="info-row">
                <span class="info-label">Cân nặng:</span>
                <span class="info-value"><xsl:value-of select="$ttChung/CanNang"/></span>
                <span class="info-label" style="margin-left: 20px;">Cao:</span>
                <span class="info-value"><xsl:value-of select="$ttChung/Cao"/></span>
                <span class="info-label" style="margin-left: 20px;">Khoa/Phòng:</span>
                <span class="info-value"><xsl:value-of select="$ttChung/KhoaPhongTen"/></span>
            </div>
            <div class="info-row">
                <span class="info-label">Chẩn đoán:</span>
                <span class="info-value"><xsl:value-of select="$ttChung/ChanDoan"/></span>
            </div>
            <xsl:if test="$ttChung/ChuThich">
                <div class="info-row">
                    <span class="info-label">Chú thích:</span>
                    <span class="info-value"><xsl:value-of select="$ttChung/ChuThich"/></span>
                </div>
            </xsl:if>
        </div>
        
        <!-- Main Observation Table -->
        <xsl:apply-templates select="$pages"/>
        
        <!-- Signature Section -->
        <div class="signature-section">
            <xsl:apply-templates select="Signature"/>
        </div>
    </xsl:template>
    
    <xsl:template match="Page">
        <xsl:variable name="maxIndex" select="46"/>
        
        <!-- Build date/time columns -->
        <xsl:variable name="dateTimeColumns">
            <xsl:for-each select="*[starts-with(local-name(), 'Ngay') or starts-with(local-name(), 'ThoiGian')]">
                <xsl:variable name="index" select="substring-after(local-name(), substring-before(local-name(), '0'))"/>
                <xsl:if test="position() &lt;= $maxIndex + 1">
                    <col>
                        <xsl:attribute name="index">
                            <xsl:value-of select="substring-after(local-name(), 'Ngay')"/>
                            <xsl:if test="not(contains(local-name(), 'Ngay'))">
                                <xsl:value-of select="substring-after(local-name(), 'ThoiGian')"/>
                            </xsl:if>
                        </xsl:attribute>
                        <xsl:value-of select="."/>
                    </col>
                </xsl:if>
            </xsl:for-each>
        </xsl:variable>
        
        <table>
            <!-- Header Row with Dates/Times -->
            <thead>
                <tr>
                    <th class="row-label">Nội dung</th>
                    <xsl:for-each select="*[starts-with(local-name(), 'Ngay')]">
                        <xsl:variable name="pos" select="position() - 1"/>
                        <xsl:if test="$pos &lt;= $maxIndex">
                            <th class="date-time-header">
                                <xsl:value-of select="."/>
                                <br/>
                                <xsl:value-of select="../*[local-name() = concat('ThoiGian', $pos)]"/>
                            </th>
                        </xsl:if>
                    </xsl:for-each>
                </tr>
            </thead>
            <tbody>
                <!-- Vital Signs Section -->
                <tr>
                    <td class="row-label">Mạch (lần/phút)</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'Mach'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Nhiệt độ (°C)</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'NhietDo'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Huyết áp (mmHg)</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'HuyetAp'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Phân cấp CS</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'PhanCapCS'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Neurological Assessment -->
                <tr>
                    <td class="row-label">Màu sắc</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'mausac'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Tri giác</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'trigiac'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Lời nói</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'loinoi'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Vận động</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'vandong'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Abdominal Assessment -->
                <tr>
                    <td class="row-label">Tình trạng ổ bụng</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'tinhtrangobung'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Elimination -->
                <tr>
                    <td class="row-label">Tiểu tiện</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'TieuTien'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Nutrition -->
                <tr>
                    <td class="row-label">Dinh dưỡng</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'DinhDuong'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Sleep/Rest -->
                <tr>
                    <td class="row-label">Giấc ngủ/Nghỉ ngơi</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'GiacNguNghiNgoi'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Hygiene -->
                <tr>
                    <td class="row-label">Vệ sinh cá nhân</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'vesinhcanhan'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Mental Status -->
                <tr>
                    <td class="row-label">Tinh thần</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'TinhThan'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Education -->
                <tr>
                    <td class="row-label">Giáo dục</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'giaoduc'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Risk Assessment -->
                <tr>
                    <td class="row-label">Nguy cơ ngã</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'nguyconga'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Loét</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'loet'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Cảnh báo sớm</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'canhbaosom'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Vấn đề</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'vande'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Goals -->
                <tr>
                    <td class="row-label">Mục tiêu 1.1</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'MucTieu110'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Mục tiêu 1.2</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'MucTieu120'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Care Interventions -->
                <tr>
                    <td class="row-label">Chăm sóc DD</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'ChamSocDD'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Thực hiện thuốc</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'thuchienthuoc'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Tư vấn/Giáo dục</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'tuvangiaoduc'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Bàn giao</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'BanGiao'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Pain Assessment -->
                <tr>
                    <td class="row-label">TDK - Vị trí đau</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'TDKViTriDau'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">TDK - Tính chất đau</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'TDKTinhChatDau'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">TDK - Thang đau</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'TDKThangDau'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Additional Observations -->
                <tr>
                    <td class="row-label">Dấu hiệu khác</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'dauhieukhacs'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Additional rows for other fields -->
                <xsl:call-template name="render-additional-rows">
                    <xsl:with-param name="page" select="."/>
                    <xsl:with-param name="maxIndex" select="$maxIndex"/>
                </xsl:call-template>
                
                <!-- Fluid Balance -->
                <tr>
                    <td class="row-label">Tổng xuất</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'tongxuat'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                <tr>
                    <td class="row-label">Tổng nhập</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'tongnhap'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
                
                <!-- Person Performing -->
                <tr>
                    <td class="row-label">Người thực hiện</td>
                    <xsl:call-template name="render-cells">
                        <xsl:with-param name="prefix" select="'NguoiThucHien'"/>
                        <xsl:with-param name="maxIndex" select="$maxIndex"/>
                        <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                </tr>
            </tbody>
        </table>
    </xsl:template>
    
    <!-- Template to render cells for a given field prefix -->
    <xsl:template name="render-cells">
        <xsl:param name="prefix"/>
        <xsl:param name="maxIndex"/>
        <xsl:param name="page"/>
        
        <xsl:variable name="maxCols" select="count($page/*[starts-with(local-name(), 'Ngay')])"/>
        
        <xsl:for-each select="$page/*[starts-with(local-name(), 'Ngay')]">
            <xsl:variable name="pos" select="position() - 1"/>
            <xsl:if test="$pos &lt;= $maxIndex">
                <td>
                    <xsl:variable name="fieldName" select="concat($prefix, $pos)"/>
                    <xsl:variable name="fieldValue" select="$page/*[local-name() = $fieldName]"/>
                    <xsl:choose>
                        <xsl:when test="contains($fieldName, 'check')">
                            <xsl:if test="$fieldValue = 'true' or $fieldValue = '1' or $fieldValue = 'x' or $fieldValue = 'X'">✓</xsl:if>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="$fieldValue"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </td>
            </xsl:if>
        </xsl:for-each>
    </xsl:template>
    
    <!-- Template to render additional rows for other fields -->
    <xsl:template name="render-additional-rows">
        <xsl:param name="page"/>
        <xsl:param name="maxIndex"/>
        
        <!-- Add rows for other specific fields if needed -->
        <xsl:if test="$page/*[starts-with(local-name(), 'liet')]">
            <tr>
                <td class="row-label">Liệt</td>
                <xsl:call-template name="render-cells">
                    <xsl:with-param name="prefix" select="'liet'"/>
                    <xsl:with-param name="maxIndex" select="$maxIndex"/>
                    <xsl:with-param name="page" select="$page"/>
                </xsl:call-template>
            </tr>
        </xsl:if>
        
        <xsl:if test="$page/*[starts-with(local-name(), 'hotrovandong')]">
            <tr>
                <td class="row-label">Hỗ trợ vận động</td>
                <xsl:call-template name="render-cells">
                    <xsl:with-param name="prefix" select="'hotrovandong'"/>
                    <xsl:with-param name="maxIndex" select="$maxIndex"/>
                    <xsl:with-param name="page" select="$page"/>
                </xsl:call-template>
            </tr>
        </xsl:if>
        
        <xsl:if test="$page/*[starts-with(local-name(), 'timmach')]">
            <tr>
                <td class="row-label">Tim mạch</td>
                <xsl:call-template name="render-cells">
                    <xsl:with-param name="prefix" select="'timmach'"/>
                    <xsl:with-param name="maxIndex" select="$maxIndex"/>
                    <xsl:with-param name="page" select="$page"/>
                </xsl:call-template>
            </tr>
        </xsl:if>
        
        <xsl:if test="$page/*[starts-with(local-name(), 'nghingoi')]">
            <tr>
                <td class="row-label">Nghỉ ngơi</td>
                <xsl:call-template name="render-cells">
                    <xsl:with-param name="prefix" select="'nghingoi'"/>
                    <xsl:with-param name="maxIndex" select="$maxIndex"/>
                    <xsl:with-param name="page" select="$page"/>
                </xsl:call-template>
            </tr>
        </xsl:if>
        
        <xsl:if test="$page/*[starts-with(local-name(), 'phu')]">
            <tr>
                <td class="row-label">Phù</td>
                <xsl:call-template name="render-cells">
                    <xsl:with-param name="prefix" select="'phu'"/>
                    <xsl:with-param name="maxIndex" select="$maxIndex"/>
                    <xsl:with-param name="page" select="$page"/>
                </xsl:call-template>
            </tr>
        </xsl:if>
        
        <xsl:if test="$page/*[starts-with(local-name(), 'ho')]">
            <tr>
                <td class="row-label">Ho</td>
                <xsl:call-template name="render-cells">
                    <xsl:with-param name="prefix" select="'ho'"/>
                    <xsl:with-param name="maxIndex" select="$maxIndex"/>
                    <xsl:with-param name="page" select="$page"/>
                </xsl:call-template>
            </tr>
        </xsl:if>
        
        <xsl:if test="$page/*[starts-with(local-name(), 'Dom')]">
            <tr>
                <td class="row-label">Đờm</td>
                <xsl:call-template name="render-cells">
                    <xsl:with-param name="prefix" select="'Dom'"/>
                    <xsl:with-param name="maxIndex" select="$maxIndex"/>
                    <xsl:with-param name="page" select="$page"/>
                </xsl:call-template>
            </tr>
        </xsl:if>
        
        <xsl:if test="$page/*[starts-with(local-name(), 'chidinhcls')]">
            <tr>
                <td class="row-label">Chỉ định CLS</td>
                <xsl:call-template name="render-cells">
                    <xsl:with-param name="prefix" select="'chidinhcls'"/>
                    <xsl:with-param name="maxIndex" select="$maxIndex"/>
                    <xsl:with-param name="page" select="$page"/>
                </xsl:call-template>
            </tr>
        </xsl:if>
    </xsl:template>
    
    <!-- Signature Template -->
    <xsl:template match="Signature">
        <div class="signature-box">
            <p><strong>Người ký:</strong> <xsl:value-of select="SignerName"/></p>
            <p><strong>Mã:</strong> <xsl:value-of select="SignerCode"/></p>
            <p><strong>Thời gian:</strong> <xsl:value-of select="SigningTime"/></p>
            <p><strong>Tag:</strong> <xsl:value-of select="Tag"/></p>
        </div>
    </xsl:template>
    
</xsl:stylesheet>
