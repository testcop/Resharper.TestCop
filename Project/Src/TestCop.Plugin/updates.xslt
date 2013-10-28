<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl">

  <xsl:output method="xml" indent="yes"/>

  <!-- This is the latest version available -->
  <xsl:variable name="LatestMajor" select="1" />
  <xsl:variable name="LatestMinor" select="0" />
  <xsl:variable name="LatestBuild" select="0" />
  <xsl:variable name="LatestRevision" select="4" />
  
  <!-- Match the PluginLocalInfo element created by serialising the data from the category -->
  <xsl:template match="/PluginLocalInfo">
    <UpdateInfos>
      <xsl:variable name="InstalledMajor" select="PluginVersion/@Major" />
      <xsl:variable name="InstalledMinor" select="PluginVersion/@Minor" />
      <xsl:variable name="InstalledBuild" select="PluginVersion/@Build" />
      <xsl:variable name="InstalledRevision" select="PluginVersion/@Revision" />

      <!-- If we have a new version, add an <UpdateInfo /> element to tell ReSharper a new version is ready -->
      <xsl:if test="($InstalledMajor &lt; $LatestMajor) or ($InstalledMajor = $LatestMajor and $InstalledMinor &lt; $LatestMinor) or ($InstalledMajor = $LatestMajor and $InstalledMinor = $LatestMinor and $InstalledBuild &lt; $LatestBuild) or ($InstalledMajor = $LatestMajor and $InstalledMinor = $LatestMinor and $InstalledBuild = $LatestBuild and $InstalledRevision &lt; $LatestRevision)">

        <UpdateInfo>
          <InformationUri>http://testcop.codeplex.com/releases</InformationUri>
          <Title>
            <xsl:value-of select="concat('TestCop for ReSharper ', $LatestMajor, '.', $LatestMinor, '.', $LatestBuild, '.', $LatestRevision, ' Released')" />
          </Title>
          <Description>Upgrade that adds support for selecting file templates to use when creating files.<xsl:value-of select="concat($InstalledMajor, '.', $InstalledMinor, '.', $InstalledBuild, '.', $InstalledRevision)" /></Description>
          <DownloadUri>https://testcop.codeplex.com/downloads/get/698008</DownloadUri>          
          <CompanyName>TestCop</CompanyName>
          <ProductName>TestCop for Resharper</ProductName>
          <ProductVersion><xsl:value-of select="concat($LatestMajor, '.', $LatestMinor, '.', $LatestBuild, '.', $LatestRevision)"/></ProductVersion>
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

