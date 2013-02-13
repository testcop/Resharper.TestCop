<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl">

  <xsl:output method="xml" indent="yes"/>

  <!-- This is the latest version available -->
  <xsl:variable name="LatestMajor" select="0" />
  <xsl:variable name="LatestMinor" select="0" />
  <xsl:variable name="LatestBuild" select="4" />

  <!-- Match the PluginLocalInfo element created by serialising the data from the category -->
  <xsl:template match="/PluginLocalInfo">
    <UpdateInfos>
      <xsl:variable name="InstalledMajor" select="PluginVersion/@Major" />
      <xsl:variable name="InstalledMinor" select="PluginVersion/@Minor" />
      <xsl:variable name="InstalledBuild" select="PluginVersion/@Build" />

      <!-- If we have a new version, add an <UpdateInfo /> element to tell ReSharper a new version is ready -->
      <xsl:if test="($InstalledMajor &lt; $LatestMajor) or ($InstalledMajor = $LatestMajor and $InstalledMinor &lt; $LatestMinor) or ($InstalledMajor = $LatestMajor and $InstalledMinor = $LatestMinor and $InstalledBuild &lt; $LatestBuild)">

        <UpdateInfo>
          <InformationUri>http://testcop.codeplex.com/releases</InformationUri>
          <Title>
            <xsl:value-of select="concat('TestCop for ReSharper ', $LatestMajor, '.', $LatestMinor, '.', $LatestBuild, ' Released')" />
          </Title>
          <Description>A minor upgrade is available.</Description>
          <DownloadUri>
            <!--<xsl:value-of select="concat('>http://testcop.codeplex.com/releases/TestCop.', $LatestMajor, '.', $LatestMinor, '.zip')" />-->
            <xsl:value-of select="concat('http://testcop.codeplex.com/downloads/get/622317#','')" />            
          </DownloadUri>
          <CompanyName>TestCop</CompanyName>
          <ProductName>TestCop for Resharper</ProductName>
          <ProductVersion>
            <xsl:value-of select="concat($LatestMajor, '.', $LatestMinor, '.', $LatestBuild, '.0')"/>
          </ProductVersion>
          <PriceTag />
          <IsFree>true</IsFree>
          <IconData>
            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
          </IconData>
        </UpdateInfo>

      </xsl:if>
    </UpdateInfos>
  </xsl:template>

</xsl:stylesheet>
