Imports System.Reflection

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

        Dim im = New atbApi.IrrigationModule()
        TextBox1.AppendText(im.transpirationCalc(plant, Nothing, Nothing).ToString() + vbNewLine)


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
End Class
