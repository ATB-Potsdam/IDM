Imports System.Reflection

Public Class Form1
    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Dim names = atbApi.data.PlantDb.GetPlantNames()
        For i = 0 To names.Count - 1
            ComboBox1.Items.Add(names(i))
        Next

        ComboBox1.Sorted = True
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim im = New atbApi.IrrigationModule()
        Dim plant As atbApi.data.Plant = im.createPlant(ComboBox1.SelectedItem)

        For i = 1 To 3
            Dim plantSet As atbApi.data.plantValues = plant.getSet(i)
            If IsNothing(plantSet) Then
                Continue For
            End If
            For Each _property As PropertyInfo In plantSet.GetType().GetProperties()
                If _property.GetValue(plantSet, Nothing) <> Nothing Then
                    TextBox1.AppendText(i.ToString() + ": " + _property.Name + ": " + _property.GetValue(plantSet, Nothing).ToString() + vbNewLine)
                End If
            Next
        Next
        TextBox1.AppendText(im.transpirationCalc(plant, Nothing, Nothing).ToString() + vbNewLine)
    End Sub
End Class
