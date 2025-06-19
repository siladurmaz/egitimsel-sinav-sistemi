<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:q="http://www.quizsystem.com/quiz"
                exclude-result-prefixes="q">

  <xsl:output method="html" doctype-system="about:legacy-compat" encoding="UTF-8" indent="yes"/>

  <!-- Ana şablon: XML'in kökünden başlar -->
  <xsl:template match="/">
    <html>
      <head>
        <title>Sınav: <xsl:value-of select="/q:Quiz/@title"/></title>
        <style>
          body { font-family: sans-serif; }
          .quiz-container { border: 1px solid #ccc; padding: 20px; margin: 20px; border-radius: 8px; }
          .question { margin-bottom: 20px; border-bottom: 1px solid #eee; padding-bottom: 10px; }
          h1, h2 { color: #333; }
          ul { list-style-type: none; padding-left: 20px; }
        </style>
      </head>
      <body>
        <!-- Diğer şablonları uygula -->
        <xsl:apply-templates select="/q:Quiz"/>
      </body>
    </html>
  </xsl:template>

  <!-- Quiz elementi için şablon -->
  <xsl:template match="q:Quiz">
    <div class="quiz-container">
      <h1><xsl:value-of select="@title"/></h1>
      <p><i><xsl:value-of select="q:Description"/></i></p>
      <hr/>
      <!-- Question elementleri için şablonları uygula -->
      <xsl:apply-templates select="q:Questions/q:Question"/>
    </div>
  </xsl:template>

  <!-- Question elementi için şablon -->
  <xsl:template match="q:Question">
    <div class="question">
      <h2>Soru <xsl:number/>: <xsl:value-of select="q:Text"/></h2>
      <ul>
        <!-- Option elementleri için şablonları uygula -->
        <xsl:apply-templates select="q:Options/q:Option"/>
      </ul>
    </div>
  </xsl:template>

  <!-- Option elementi için şablon -->
  <xsl:template match="q:Option">
    <li>
      <input type="radio" name="q{../../@id}"/> <!-- Örnek radio button -->
      <xsl:value-of select="."/>
    </li>
  </xsl:template>

</xsl:stylesheet>