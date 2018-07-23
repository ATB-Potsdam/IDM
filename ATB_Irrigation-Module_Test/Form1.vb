Imports System.Reflection
Imports System.Threading.Tasks
Imports System.Globalization
Imports System.IO



Public Class Form1
    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Dim plantNames = atbApi.data.LocalPlantDb.Instance.getPlantNames()

        For i = 0 To plantNames.Count - 1
            ComboBox1.Items.Add(plantNames(i))
        Next

        ComboBox1.Sorted = True

        ' Add any initialization after the InitializeComponent() call.
        Dim soilNames = atbApi.data.LocalSoilDb.Instance.getSoilNames()

        For i = 0 To soilNames.Count - 1
            ComboBox2.Items.Add(soilNames(i))
        Next

        ComboBox2.Sorted = True
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim plant As atbApi.data.Plant = New atbApi.data.Plant(ComboBox1.SelectedItem)

        For i = 400 To 399
            Dim plantSet As atbApi.data.PlantValues = plant.getValues(i)
            If IsNothing(plantSet) Then
                Continue For
            End If
            For Each _property As PropertyInfo In plantSet.GetType().GetProperties()
                If _property.GetValue(plantSet, Nothing) <> Nothing Then
                    TextBox1.AppendText(i.ToString() + ": " + _property.Name + ": " + _property.GetValue(plantSet, Nothing).ToString() + vbNewLine)
                End If
            Next
        Next

        Dim seedDate As Date = DateSerial(2004, 4, 13)
        Dim harvestDate As Date = DateSerial(2004, 10, 5)
        For day = 9 To 15
            Dim actualDate As Date = DateSerial(2004, 4, day)
            Dim plantSet As atbApi.data.PlantValues = plant.getValues(actualDate, seedDate, harvestDate)
            If IsNothing(plantSet) Then
                Continue For
            End If
            For Each _property As PropertyInfo In plantSet.GetType().GetProperties()
                If _property.GetValue(plantSet, Nothing) <> Nothing Then
                    TextBox1.AppendText(actualDate.ToString() + ": " + _property.Name + ": " + _property.GetValue(plantSet, Nothing).ToString() + vbNewLine)
                End If
            Next
        Next
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim fs As System.IO.FileStream = New IO.FileStream("C:\IWRM_MIKE-Basin_Irrigation-Module\ATB_Irrigation-Module_cs\IWRM_ATB-PlantData.csv", IO.FileMode.Open)
        Dim plantDb As atbApi.data.PlantDb = New atbApi.data.PlantDb(fs)
        fs.Close()
        Dim filePlant As atbApi.data.Plant = New atbApi.data.Plant("CROPWAT_80_Crop_data_CITRUS_70p_ca_bare", plantDb)
        TextBox1.AppendText(filePlant.stageTotal.ToString() + " " + filePlant.name + vbNewLine)
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim soil As atbApi.data.Soil = New atbApi.data.Soil(ComboBox2.SelectedItem)

        For z = 0 To 2 Step 0.01
            Dim soilSet As atbApi.data.SoilValues = soil.getValues(z)
            If IsNothing(soilSet) Then
                Continue For
            End If
            For Each _property As PropertyInfo In soilSet.GetType().GetProperties()
                If _property.GetValue(soilSet, Nothing) <> Nothing Then
                    TextBox1.AppendText(z.ToString() + ": " + _property.Name + ": " + _property.GetValue(soilSet, Nothing).ToString() + vbNewLine)
                End If
            Next
        Next
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim fs As System.IO.FileStream = New IO.FileStream("C:\IWRM_MIKE-Basin_Irrigation-Module\testdata\climate_Potsdam_53c6b30845900e364c000013.date_1980-01-01T00_00_00.000Z.csv", IO.FileMode.Open)
        Dim climate As atbApi.data.Climate = New atbApi.data.Climate(fs, atbApi.data.TimeStep.day)
        fs.Close()
        TextBox1.AppendText(climate.name + " start:" + climate.start + " end:" + climate.end + vbNewLine)

        For i = 0 To 5
            Dim fs1 As System.IO.FileStream = New IO.FileStream("C:\IWRM_MIKE-Basin_Irrigation-Module\testdata\climate.uea_cru_public.date-1900-01-01T00-00-00.000Z_clean_" & i & ".csv", IO.FileMode.Open)
            Dim climate1 As atbApi.data.Climate = New atbApi.data.Climate(fs1, atbApi.data.TimeStep.month)
            fs1.Close()
            TextBox1.AppendText(climate1.name + " start:" + climate1.start + " end:" + climate1.end + vbNewLine)

        Next
    End Sub

    Private Async Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim climate As atbApi.data.Climate = New atbApi.data.Climate("12.34_-80.3", atbApi.data.TimeStep.day)
        Dim location As atbApi.tools.Location = New atbApi.tools.Location(12.34, -80.3)
        Dim count As Integer = Await climate.loadClimateByLocationTagFromATBWebService(location, Nothing, New DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc), New DateTime(2012, 12, 31, 0, 0, 0, DateTimeKind.Utc))
        TextBox1.AppendText(climate.name + " start:" + climate.start + " end:" + climate.end + vbNewLine)

        Dim climate2 As atbApi.data.Climate = New atbApi.data.Climate("12.34_-80.3_uea", atbApi.data.TimeStep.month)
        Dim count2 As Integer = Await climate2.loadClimateByLocationTagFromATBWebService(location, "uea_cru_public", New DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc), New DateTime(2012, 12, 31, 0, 0, 0, DateTimeKind.Utc))
        TextBox1.AppendText(climate2.name + " start:" + climate2.start + " end:" + climate2.end + vbNewLine)
    End Sub

    Private Async Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Dim location As atbApi.tools.Location = New atbApi.tools.Location(48.5, 9.3)
        Dim climate As atbApi.data.Climate = New atbApi.data.Climate("48.5_9.3", atbApi.data.TimeStep.day)
        Dim count As Integer = Await climate.loadClimateByLocationTagFromATBWebService(location, Nothing, New DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc), New DateTime(2012, 12, 31, 0, 0, 0, DateTimeKind.Utc))
        Dim altitude As Double = Await climate.loadAltitudeFromATBWebService(location)
        location.alt = altitude
        TextBox1.AppendText(climate.name + " start:" + climate.start + " end:" + climate.end + " altitude:" + location.alt.ToString() + vbNewLine)
        Dim testDate As DateTime = New DateTime(2012, 7, 31, 0, 0, 0, DateTimeKind.Utc)
        Dim et0Hg As atbApi.Et0Result = New atbApi.Et0Result
        atbApi.Et0.Et0CalcHg(climate, testDate, location, New atbApi.Et0HgArgs, et0Hg)
        Dim et0Pm As atbApi.Et0Result = New atbApi.Et0Result
        atbApi.Et0.Et0CalcPm(climate, testDate, location, New atbApi.Et0PmArgs, et0Pm)
        Dim et0 As atbApi.Et0Result = New atbApi.Et0Result
        atbApi.Et0.Et0Calc(climate, testDate, location, et0)

        TextBox1.AppendText("et0Hg:" + et0Hg.et0.ToString() + " et0Pm:" + et0Pm.et0.ToString() + " et0:" + et0.et0.ToString() + " climateEt0:" + climate.getValues(testDate).et0.ToString() + vbNewLine)
    End Sub




    Private Async Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        'Dim climate As atbApi.data.Climate = New atbApi.data.Climate("DWD_03987_Potsdam", atbApi.data.TimeStep.day, "53c6b30845900e364c000013")
        Dim climate As atbApi.data.Climate = New atbApi.data.Climate("UEA_CRU_timeseries_v3.21_52.25_32.75", atbApi.data.TimeStep.day, "53506649594fa52d48007088")

        'define interval to load climate data
        'Dim climateStart As DateTime = New DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        'Dim climateEnd As DateTime = New DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Utc)
        Dim climateStart As DateTime = New DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        Dim climateEnd As DateTime = New DateTime(2001, 12, 31, 0, 0, 0, DateTimeKind.Utc)
        'load data asyncron from webservice
        Dim count As Integer = Await climate.loadClimateByIdFromATBWebService(climateStart, climateEnd)

        'load altitude from webservice
        'Dim altitude As Double = Await climate.loadAltitudeFromATBWebService(climate.location)
        'create plant from dll internal plant database
        Dim plant As atbApi.data.Plant = New atbApi.data.Plant("IWRM_wheat_III")
        'create soil from dll internal standard soils
        Dim soil As atbApi.data.Soil = New atbApi.data.Soil("Iran_IWRM_ESF19")
        'define seedDate and harvestDate
        'Dim seedDate As DateTime = New DateTime(2015, 4, 12, 0, 0, 0, DateTimeKind.Utc)
        'Dim harvestDate As DateTime = New DateTime(2015, 10, 5, 0, 0, 0, DateTimeKind.Utc)
        Dim seedDate As DateTime = New DateTime(2000, 7, 1, 0, 0, 0, DateTimeKind.Utc)
        Dim harvestDate As DateTime = New DateTime(2001, 4, 15, 0, 0, 0, DateTimeKind.Utc)
        'start calculation

        Dim etArgs As atbApi.ETArgs = New atbApi.ETArgs
        etArgs.climate = climate
        etArgs.soil = soil
        etArgs.plant = plant
        etArgs.seedDate = seedDate
        etArgs.harvestDate = harvestDate
        etArgs.location = climate.location
        Dim etResult As atbApi.ETResult = Nothing
        Dim startDate As DateTime = New DateTime(seedDate.Year, seedDate.Month, 1, 0, 0, 0, seedDate.Kind)
        Dim endDate As DateTime = New DateTime(seedDate.Year, seedDate.Month, DateTime.DaysInMonth(seedDate.Year, seedDate.Month), 0, 0, 0, seedDate.Kind)
        'etArgs.autoIrr = New atbApi.data.AutoIrrigationControl()
        'etArgs.autoIrr = New atbApi.data.AutoIrrigationControl(level:=0, cutoff:=0.15, deficit:=0.2)
        'etArgs.autoIrr = New atbApi.data.AutoIrrigationControl(level:=0, cutoff:=0.1)

        Do
            etArgs.start = startDate
            etArgs.end = endDate
            atbApi.Transpiration.ETCalc(etArgs, etResult, True)
            startDate = startDate.AddMonths(1)
            endDate = endDate.AddMonths(1)
            endDate = New DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month), 0, 0, 0, endDate.Kind)
        Loop While startDate < harvestDate
        TextBox1.AppendText("et0:" + etResult.et0.ToString() + " runtimeMs:" + etResult.runtimeMs.ToString("F3") + vbNewLine)
        For Each item In etResult.dailyValues
            TextBox1.AppendText(item.Key.ToString() + ": dpRz: " + item.Value.dpRz.ToString() + " tAct: " + item.Value.tAct.ToString() + vbNewLine)
        Next
    End Sub


    Private Async Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        'load climate names from http
        Dim climateNames = Await atbApi.data.ClimateWebServiceDb.GetClimateNames()
        For i = 0 To climateNames.Count - 1
            ComboBox3.Items.Add(climateNames(i))
        Next

        ComboBox3.Sorted = True
    End Sub

    Private Sub ComboBox3_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox3.SelectedIndexChanged
        Dim seedDate As DateTime = New DateTime(2012, 4, 12, 0, 0, 0, DateTimeKind.Utc)
        Dim harvestDate As DateTime = New DateTime(2012, 10, 5, 0, 0, 0, DateTimeKind.Utc)
        Dim testClimate As Task(Of atbApi.data.Climate) = atbApi.data.ClimateWebServiceDb.GetClimate(ComboBox3.SelectedItem, seedDate, harvestDate, atbApi.data.TimeStep.day)

    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        'transpiration calc DHI
        'create CultureInfo for english ans german formatted csv data
        Dim cultureInfoEn As CultureInfo = New CultureInfo("en-US")
        Dim cultureInfoDe As CultureInfo = New CultureInfo("de-DE")
        'create empty climateDb
        Dim climateDb As atbApi.data.ClimateDb = New atbApi.data.ClimateDb()
        'load 6 climate stations which are referenced in the cropSequence
        For i = 0 To 5
            Dim climateFile = "..\..\..\testdata\UEA_CRU-ClimateData_" & i & ".csv"
            Dim climateStream As FileStream = File.OpenRead(climateFile)
            'load climate data from file into ram, TimeStep is required to convert monthly sums to daily values
            climateDb.addClimate(climateStream, atbApi.data.TimeStep.month, cultureInfoEn)
        Next
        'additionally load synoptic climate stations which are referenced in the cropSequence
        'For Each climateFile In Directory.GetFiles("..\..\..\testdata", "IRAN_?????_*.date*")
        For Each climateFile In Directory.GetFiles("..\..\..\testdata", "IWRM-Synoptic-final-ClimateData_?.csv.gz")
            Dim climateStream As FileStream = File.OpenRead(climateFile)
            If climateFile.EndsWith(".csv", StringComparison.InvariantCulture) Or climateFile.EndsWith(".gz", StringComparison.InvariantCulture) Then
                'load climate data from file into ram
                climateDb.addClimate(climateStream, atbApi.data.TimeStep.day, cultureInfoEn)
            End If
        Next

        'create empty rainPatternDb
        Dim rainPatternDb As atbApi.data.RainPatternDb = New atbApi.data.RainPatternDb()
        'additionally load rain pattern which are referenced in the cropSequence
        For Each rainPatternFile In Directory.GetFiles("..\..\..\testdata", "IRAN_?????_*_rainPattern*")
            Dim rainPatternStream As FileStream = File.OpenRead(rainPatternFile)
            If rainPatternFile.EndsWith(".csv", StringComparison.InvariantCulture) Or rainPatternFile.EndsWith(".gz", StringComparison.InvariantCulture) Then
                'load rainPattern data from file into ram
                rainPatternDb.addRainPattern(rainPatternStream, cultureInfoEn)
            End If
        Next

        'cropSequence 1000 years from 1901 to 2900
        'Dim cSFile = "..\..\..\testdata\IWRM_cropSequence_Scenario_1_1000years.csv.gz"
        'Dim cSFile = "..\..\..\testdata\IWRM_cropSequence_Scenario_1.csv"
        Dim cSFile = "..\..\..\testdata\cS_IWRM_Scenario_4.csv.gz"
        Dim cSStream As FileStream = File.OpenRead(cSFile)
        'create cropSequence, use builtin plant and soil db and before created climate db
        Dim cS As atbApi.data.CropSequence = _
            New atbApi.data.CropSequence(cSStream, atbApi.data.LocalPlantDb.Instance, atbApi.data.LocalSoilDb.Instance, climateDb, rainPatternDb, cultureInfoEn)

        'common arguments for all calculations
        Dim etArgs As New atbApi.ETArgs()
        'create autoIrrigation parameters unnessecary, now contained in cropSequence
        etArgs.autoIrr = New atbApi.data.AutoIrrigationControl(level:=0, cutoff:=0.15, deficit:=0.2)
        'loop over 1000 years
        Dim loopDate As DateTime = New DateTime(1996, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        Dim loopEnd As DateTime = New DateTime(2010, 12, 31, 0, 0, 0, DateTimeKind.Utc)
        'Dim loopDate As DateTime = New DateTime(1901, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        'Dim loopEnd As DateTime = New DateTime(2900, 12, 31, 0, 0, 0, DateTimeKind.Utc)
        Dim result As atbApi.data.CropSequenceResult

        Do
            'set endDate of one loop to last day of month
            Dim endDate As DateTime = New DateTime(loopDate.Year, loopDate.Month, DateTime.DaysInMonth(loopDate.Year, loopDate.Month), 0, 0, 0, loopDate.Kind)
            'calculate irrigation demand and don't cumulate results and plant development -> dryRun:=True
            result = cS.runCropSequence(start:=loopDate, end:=endDate, etArgs:=etArgs, irrigationAmount:=Nothing, dryRun:=True)
            'modify irrigation demand, here just divide by 2 as an example
            Dim resultKeys As List(Of String) = New List(Of String)(result.networkIdIrrigationDemand.Keys)
            'For Each resultKey In resultKeys
            'result.networkIdIrrigationDemand(resultKey).irrigationDemand.surfaceWater.amount /= 2
            'Next
            'calculate again with real irrigation amount -> mbResult:=result, dryRun:=False
            result = cS.runCropSequence(start:=loopDate, end:=endDate, etArgs:=etArgs, irrigationAmount:=result, dryRun:=False)
            'calculate next timestep
            loopDate = loopDate.AddMonths(1)
        Loop While loopDate < loopEnd

        Dim totalAutoNetIrr As Double = 0
        Dim totalArea As Double = 0
        For Each item In cS.results
            totalAutoNetIrr = totalAutoNetIrr + item.Value.autoNetIrrigation * cS.areas(item.Key) * 1000000
            totalArea = totalArea + cS.areas(item.Key) * 1000000
            Dim networkRightField As Array = item.Key.Split(".")
            TextBox1.AppendText("network: " + networkRightField(0) + " right: " + networkRightField(1) + " fieldId: " + networkRightField(2) + " autoNetIrr: " + item.Value.autoNetIrrigation.ToString() + vbNewLine)
        Next
        TextBox1.AppendText("total area: " + totalArea.ToString() + vbNewLine)
        TextBox1.AppendText("total amount autoNetIrr: " + totalAutoNetIrr.ToString())
    End Sub
End Class
