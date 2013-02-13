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
          <DownloadUri>http://testcop.codeplex.com/downloads/get/622317</DownloadUri>          
          <CompanyName>TestCop</CompanyName>
          <ProductName>TestCop for Resharper</ProductName>
          <ProductVersion><xsl:value-of select="concat($LatestMajor, '.', $LatestMinor, '.', $LatestBuild, '.0')"/></ProductVersion>
          <PriceTag />
          <IsFree>true</IsFree>
          <IconData>
            iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAABGdBTUEAAK/INwWK6QAAABl0RVh0U29mdHdhcmUAQWRvYmUgSW1hZ2VSZW
            FkeXHJZTwAAAO9SURBVHjaYvz//z8DJQAggBgZWHvAFMOfvwzMrMxAJiPD35+/GRiYWWxk9OXm/vh85dub2wfCGHj4bzP8es/AKGLI8P/t
            RgaGnw/BBgAEEDMDsxvEgH//GZhYmICa/zKwcLB0WIQZTAsqcRCXMTWXePvmf/zHhzeeMvz+eImRV46B4fsNBoa/H8EGAAQQigH///zVEV
            AQ2emWaxtiHqjL8urzX4bfTAwM8sZG7H/YRP3ePnkjD/TwJoafd+AGAAQQxAv/wEaUqDqrNZmE6HEyiAgzPH36i+HXu08MDBwcDMx8XAz8
            HP8ZXtx6xnB968Zb365O9mX4++wWyACAAIIY8Oeflbqf3kHVAH2WN+9/MMidW8agz3WOQUfuDsPnr5IML9/JM2wXjmB4L6nN8PvTP4Zbyz
            of/LjarwnU/wMggFiAmhk4pPjbeA1kWO49+czw6BUTQ4DQNYbysrUMDLxA9/+/yPBmMyfD0sOBDI8YvzMIcDEw8Bj7Kvy4vTKN4dezSQAB
            xMTAymTFoS5l9ez7P4aHr74yfP39n+H8DyWg2fwMDDeNGBg+CjFcfK/KcOe7CMOHz18Znr38yPBNUIaBUdEtA+gCNoAAYmEWFej8IcbL+g
            no9H//gIHxm43hx5+fDAxMwECSeMrAwPmJ4c9HJoY/338z/GH/yfDn/y+gNqDL1LzUGB7uDAMIIJZ/IhyGPzjZgfEK1PTvLwPDL2YGti8v
            Gd4Dg4hP+zkDwyNgknj5jYHlOyhAGUHeBsYAMC54pJgZeGXCAAKI5f+Lg+cZPivYMLAADfjPBoyOfwwn/1kzlE/+x2Ak8oThwzcBhuM/dB
            m+gmz9DXTh/78Q/PrGf4aPd24BBBAzw0/Wlwwc6hGMUspM4iK/GGz4rjGYK/9i4JZVYfgposfwS0GfQUBeiEGD/w0DMw8bwztOKYY/X4GG
            XVn8ieHNmXSAAAJGoyrQjdILxDUNIvJjFNmleYHeZ+Nm4OUTAKZeAQY+Xh4GFiZGhn/AcPn16yfD5A2XGZavP/6Z4enuaQy/PlQABBAzA4
            sk0N83Nnx/fmgL47+/XsrqOvzCwqLA9MPJwMXFxcDNzcPABzSImZWDYfuufQxr5/Q8+PTktCfD3x9LQAkJIIAYGdi0gYHynIHp3ztQapZX
            UVFdZG9vZ6WiosIiLCzMwMzMzPD27VuGw0eO/N69a9epHz9+ZAH1XYLlRoAAghvA8v8dKE0xMDExATMkozM7O7svKyurLAMGqZgJqe//79
            eydQ/VYg/oOcnQECDACbA1XEBsc6LgAAAABJRU5ErkJggg==
          </IconData>
        </UpdateInfo>

      </xsl:if>
    </UpdateInfos>
  </xsl:template>

</xsl:stylesheet>
